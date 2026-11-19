const express = require('express');
const cors = require('cors');
const path = require('path');
const bcrypt = require('bcryptjs');
const nodemailer = require('nodemailer');
require('dotenv').config();
const { Pool } = require('pg');

const app = express();
app.use(cors());
app.use(express.json({ limit: '10mb' }));
app.use(express.static(path.join(__dirname)));

const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

// Configuración de correo (Nodemailer con Gmail)
const transporter = nodemailer.createTransport({
  service: 'gmail',
  auth: {
    user: process.env.EMAIL_USER,
    pass: process.env.EMAIL_PASS
  }
});

// Almacén temporal de códigos de recuperación (correo -> { codigo, expira })
const codigosRecuperacion = new Map();

// ==========================================
// 1. PRODUCTOS Y CATÁLOGO
// ==========================================
app.get('/api/productos', async (req, res) => {
  try {
    const result = await pool.query(`
      SELECT 
        p.id_producto AS id,
        p.nombre,
        p.precio_actual AS precio,
        COALESCE(c.nombre, 'otro') AS categoria,
        COALESCE(s.nombre, 'General') AS sucursal,
        COALESCE(s.direccion, '') AS "sucursalDireccion",
        COALESCE(s.es_cadena_national, false) AS "esCadena",
        p.imagen_url AS imagen,
        p.activo
      FROM producto p
      LEFT JOIN categoria c ON p.id_categoria = c.id_categoria
      LEFT JOIN sucursal s ON p.id_sucursal = s.id_sucursal
      ORDER BY p.id_producto DESC
    `);
    res.json(result.rows);
  } catch (err) {
    console.error("Error en GET /api/productos:", err.message);
    res.status(500).json({ error: err.message });
  }
});

app.post('/api/productos', async (req, res) => {
  const { nombre, precio, categoria, sucursal, esCadena, imagen, activo } = req.body;
  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    let sucRes = await client.query('SELECT id_sucursal FROM sucursal WHERE LOWER(TRIM(nombre)) = LOWER(TRIM($1))', [sucursal]);
    let idSucursal = sucRes.rows.length > 0
      ? sucRes.rows[0].id_sucursal
      : (await client.query('INSERT INTO sucursal (nombre, es_cadena_national) VALUES ($1, $2) RETURNING id_sucursal', [sucursal, esCadena || false])).rows[0].id_sucursal;

    let catRes = await client.query('SELECT id_categoria FROM categoria WHERE LOWER(TRIM(nombre)) = LOWER(TRIM($1))', [categoria]);
    let idCategoria = catRes.rows.length > 0
      ? catRes.rows[0].id_categoria
      : (await client.query('INSERT INTO categoria (nombre) VALUES ($1) RETURNING id_categoria', [categoria])).rows[0].id_categoria;

    const prodRes = await client.query(
      `INSERT INTO producto (id_sucursal, id_categoria, nombre, precio_actual, imagen_url, activo) 
       VALUES ($1, $2, $3, $4, $5, $6) RETURNING id_producto`,
      [idSucursal, idCategoria, nombre, precio, imagen, activo ?? true]
    );

    await client.query('COMMIT');
    res.json({ success: true, id: prodRes.rows[0].id_producto });
  } catch (err) {
    await client.query('ROLLBACK');
    res.status(500).json({ error: err.message });
  } finally {
    client.release();
  }
});

app.put('/api/productos/:id', async (req, res) => {
  const { id } = req.params;
  const { nombre, precio, categoria, imagen, activo } = req.body;
  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    let catRes = await client.query('SELECT id_categoria FROM categoria WHERE LOWER(TRIM(nombre)) = LOWER(TRIM($1))', [categoria]);
    let idCategoria = catRes.rows.length > 0
      ? catRes.rows[0].id_categoria
      : (await client.query('INSERT INTO categoria (nombre) VALUES ($1) RETURNING id_categoria', [categoria])).rows[0].id_categoria;

    await client.query(
      `UPDATE producto SET id_categoria = $1, nombre = $2, precio_actual = $3, imagen_url = $4, activo = $5 
       WHERE id_producto = $6`,
      [idCategoria, nombre, precio, imagen, activo, id]
    );

    await client.query('COMMIT');
    res.json({ success: true });
  } catch (err) {
    await client.query('ROLLBACK');
    res.status(500).json({ error: err.message });
  } finally {
    client.release();
  }
});

app.delete('/api/productos/:id', async (req, res) => {
  try {
    await pool.query('DELETE FROM producto WHERE id_producto = $1', [req.params.id]);
    res.json({ message: 'Producto eliminado' });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// ==========================================
// 2. AUTENTICACIÓN, REGISTRO Y RECUPERACIÓN
// ==========================================

// Registro con contraseña encriptada (bcrypt)
app.post('/api/usuarios', async (req, res) => {
  const { correo, password, nombre, rol, sucursal, direccion, telefono, esCadena, metodoPago } = req.body;
  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    const existe = await client.query('SELECT id_usuario FROM usuario WHERE LOWER(TRIM(correo)) = LOWER(TRIM($1))', [correo]);
    if (existe.rows.length > 0) {
      await client.query('ROLLBACK');
      return res.status(400).json({ error: 'Este correo ya está registrado.' });
    }

    let idSucursal = null;
    if (sucursal) {
      let sucRes = await client.query('SELECT id_sucursal FROM sucursal WHERE LOWER(TRIM(nombre)) = LOWER(TRIM($1))', [sucursal]);
      idSucursal = sucRes.rows.length > 0
        ? sucRes.rows[0].id_sucursal
        : (await client.query('INSERT INTO sucursal (nombre, direccion, telefono, es_cadena_national) VALUES ($1, $2, $3, $4) RETURNING id_sucursal', [sucursal, direccion || '', telefono || '', esCadena || false])).rows[0].id_sucursal;
    }

    const hash = await bcrypt.hash(password, 10);
    const metodoGuardar = rol === 'cliente' ? (metodoPago || 'Efectivo') : null;

    await client.query(
      `INSERT INTO usuario (id_sucursal, nombre, correo, contrasena_hash, rol, direccion_predeterminada, metodo_pago_predeterminada) 
       VALUES ($1, $2, $3, $4, $5, $6, $7)`,
      [idSucursal, nombre, correo, hash, rol, direccion || '', metodoGuardar]
    );

    await client.query('COMMIT');
    res.json({ success: true, correo, nombre, rol, sucursal, direccion: direccion || '', metodoPago: metodoGuardar });
  } catch (err) {
    await client.query('ROLLBACK');
    res.status(500).json({ error: err.message });
  } finally {
    client.release();
  }
});

// Login seguro en servidor
app.post('/api/login', async (req, res) => {
  const { correo, password, rol } = req.body;
  try {
    const result = await pool.query(`
      SELECT 
        u.id_usuario AS id,
        u.nombre,
        u.correo,
        u.contrasena_hash,
        u.rol,
        s.nombre AS sucursal,
        COALESCE(u.direccion_predeterminada, '') AS direccion,
        COALESCE(u.metodo_pago_predeterminada, 'Efectivo') AS "metodoPago",
        COALESCE(s.es_cadena_national, false) AS "esCadena"
      FROM usuario u
      LEFT JOIN sucursal s ON u.id_sucursal = s.id_sucursal
      WHERE LOWER(TRIM(u.correo)) = LOWER(TRIM($1)) AND u.rol = $2
    `, [correo, rol]);

    if (result.rows.length === 0) {
      return res.status(401).json({ error: 'Credenciales incorrectas o el tipo de usuario no coincide.' });
    }

    const user = result.rows[0];
    let passwordValida = false;

    // Verificar si ya es un hash bcrypt o si es una cuenta antigua en texto plano
    if (user.contrasena_hash.startsWith('$2a$') || user.contrasena_hash.startsWith('$2b$')) {
      passwordValida = await bcrypt.compare(password, user.contrasena_hash);
    } else if (user.contrasena_hash === password) {
      passwordValida = true;
      // Migrar automáticamente la contraseña antigua a hash encriptado en Neon
      const nuevoHash = await bcrypt.hash(password, 10);
      await pool.query('UPDATE usuario SET contrasena_hash = $1 WHERE id_usuario = $2', [nuevoHash, user.id]);
    }

    if (!passwordValida) {
      return res.status(401).json({ error: 'Credenciales incorrectas.' });
    }

    // Devolver datos del usuario SIN el hash de la contraseña
    delete user.contrasena_hash;
    res.json(user);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// Actualizar perfil de cliente sin tocar contraseña ni saltar IDs
app.put('/api/usuarios/perfil', async (req, res) => {
  const { correo, nombre, direccion, metodoPago } = req.body;
  try {
    await pool.query(
      `UPDATE usuario 
       SET nombre = $1, direccion_predeterminada = $2, metodo_pago_predeterminada = $3 
       WHERE LOWER(TRIM(correo)) = LOWER(TRIM($4))`,
      [nombre, direccion || '', metodoPago || 'Efectivo', correo]
    );
    res.json({ success: true });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// Paso 1 Recuperación: Generar y enviar código de 6 dígitos por correo
app.post('/api/auth/forgot-password', async (req, res) => {
  const { correo } = req.body;
  try {
    const userRes = await pool.query('SELECT nombre FROM usuario WHERE LOWER(TRIM(correo)) = LOWER(TRIM($1))', [correo]);
    if (userRes.rows.length === 0) {
      return res.status(404).json({ error: 'No existe ninguna cuenta asociada a este correo.' });
    }

    const codigo = Math.floor(100000 + Math.random() * 900000).toString();
    codigosRecuperacion.set(correo.toLowerCase().trim(), {
      codigo,
      expira: Date.now() + 15 * 60 * 1000 // 15 minutos
    });

    if (process.env.EMAIL_USER && process.env.EMAIL_PASS) {
      await transporter.sendMail({
        from: `"MenuDirect Soporte" <${process.env.EMAIL_USER}>`,
        to: correo,
        subject: 'Código de recuperación de contraseña - MenuDirect',
        html: `
          <div style="font-family: Arial, sans-serif; padding: 20px; color: #333;">
            <h2 style="color: #f26522;">MenuDirect</h2>
            <p>Hola <strong>${userRes.rows[0].nombre}</strong>,</p>
            <p>Recibimos una solicitud para restablecer tu contraseña. Usa el siguiente código de verificación (válido por 15 minutos):</p>
            <div style="font-size: 28px; font-weight: bold; letter-spacing: 6px; background: #f8f9fa; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0;">
              ${codigo}
            </div>
            <p style="font-size: 12px; color: #777;">Si no solicitaste este cambio, puedes ignorar este correo.</p>
          </div>
        `
      });
    } else {
      console.log(`[MODO PRUEBA] Código de recuperación para ${correo}: ${codigo}`);
    }

    res.json({ success: true, message: 'Código enviado al correo.' });
  } catch (err) {
    console.error("Error al enviar correo:", err.message);
    res.status(500).json({ error: 'No se pudo enviar el correo. Verifica las credenciales SMTP en .env.' });
  }
});

// Paso 2 Recuperación: Validar código y guardar nueva contraseña encriptada
app.post('/api/auth/reset-password', async (req, res) => {
  const { correo, codigo, nuevaPassword } = req.body;
  const registro = codigosRecuperacion.get(correo.toLowerCase().trim());

  if (!registro || registro.codigo !== codigo.trim()) {
    return res.status(400).json({ error: 'El código ingresado es incorrecto.' });
  }
  if (Date.now() > registro.expira) {
    codigosRecuperacion.delete(correo.toLowerCase().trim());
    return res.status(400).json({ error: 'El código ha expirado. Solicita uno nuevo.' });
  }

  try {
    const nuevoHash = await bcrypt.hash(nuevaPassword, 10);
    await pool.query(
      'UPDATE usuario SET contrasena_hash = $1 WHERE LOWER(TRIM(correo)) = LOWER(TRIM($2))',
      [nuevoHash, correo]
    );
    codigosRecuperacion.delete(correo.toLowerCase().trim());
    res.json({ success: true });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

// ==========================================
// 3. PEDIDOS Y DETALLES
// ==========================================
app.get('/api/pedidos', async (req, res) => {
  try {
    const pedidosRes = await pool.query(`
      SELECT 
        id_pedido AS id,
        COALESCE(cliente_nombre, 'Cliente Web') AS cliente,
        COALESCE(cliente_telefono, 'S/T') AS telefono,
        direccion_envio AS direccion,
        metodo_pago AS "metodoPago",
        total_pedido AS "totalGeneral",
        fecha_hora AS "creadoEn"
      FROM pedido
      ORDER BY fecha_hora DESC
    `);

    const detallesRes = await pool.query(`
      SELECT 
        pd.id_pedido AS "idOrden",
        pd.id_producto AS "idProducto",
        pd.nombre_producto_historico AS nombre,
        COALESCE(s.nombre, 'General') AS sucursal,
        pd.cantidad,
        pd.subtotal,
        COALESCE(pd.estado, 'Recibido') AS estado
      FROM pedido_detalle pd
      LEFT JOIN producto p ON pd.id_producto = p.id_producto
      LEFT JOIN sucursal s ON p.id_sucursal = s.id_sucursal
    `);

    const pedidos = pedidosRes.rows.map(p => ({
      id: p.id,
      cliente: p.cliente,
      telefono: p.telefono,
      direccion: p.direccion,
      metodoPago: p.metodoPago,
      totalGeneral: parseFloat(p.totalGeneral),
      creadoEn: p.creadoEn,
      detalles: detallesRes.rows
        .filter(d => d.idOrden === p.id)
        .map(d => ({
          idProducto: d.idProducto,
          nombre: d.nombre,
          sucursal: d.sucursal,
          cantidad: d.cantidad,
          subtotal: parseFloat(d.subtotal),
          estado: d.estado
        }))
    }));

    res.json(pedidos);
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

app.post('/api/pedidos', async (req, res) => {
  const { id, cliente, telefono, direccion, metodoPago, totalGeneral, detalles } = req.body;
  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    await client.query(
      `INSERT INTO pedido (id_pedido, cliente_nombre, cliente_telefono, fecha_hora, estado, direccion_envio, metodo_pago, total_pedido) 
       VALUES ($1, $2, $3, CURRENT_TIMESTAMP, 'Recibido', $4, $5, $6)`,
      [id, cliente, telefono, direccion, metodoPago, totalGeneral]
    );

    for (const item of detalles) {
      await client.query(
        `INSERT INTO pedido_detalle (id_pedido, id_producto, nombre_producto_historico, precio_unitario_historico, cantidad, subtotal, estado) 
         VALUES ($1, $2, $3, $4, $5, $6, $7)`,
        [id, item.idProducto, item.nombre, item.subtotal / item.cantidad, item.cantidad, item.subtotal, item.estado || 'Recibido']
      );
    }

    await client.query('COMMIT');
    res.json({ success: true, idOrden: id });
  } catch (err) {
    await client.query('ROLLBACK');
    res.status(500).json({ error: err.message });
  } finally {
    client.release();
  }
});

app.put('/api/pedidos/:id/estado', async (req, res) => {
  const { id } = req.params;
  const { sucursal, estado } = req.body;
  try {
    await pool.query(`
      UPDATE pedido_detalle pd
      SET estado = $1
      FROM producto p, sucursal s
      WHERE pd.id_producto = p.id_producto 
        AND p.id_sucursal = s.id_sucursal
        AND pd.id_pedido = $2 
        AND LOWER(TRIM(s.nombre)) = LOWER(TRIM($3))
    `, [estado, id, sucursal]);

    res.json({ success: true });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`MenuDirect corriendo en http://localhost:${PORT}`);
});