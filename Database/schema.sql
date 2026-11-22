-- ============================================================================
-- PROYECTO: MenuDirect
-- MOTOR: PostgreSQL (Neon Cloud)
-- ARCHIVO: Database/schema.sql
-- DESCRIPCIÓN: Creación de tablas, llaves primarias/foráneas, restricciones e índices
-- ============================================================================

-- Limpieza previa en orden inverso a las dependencias para permitir re-ejecución
DROP TABLE IF EXISTS detalle_pedido CASCADE;
DROP TABLE IF EXISTS pedidos CASCADE;
DROP TABLE IF EXISTS detalle_carrito CASCADE;
DROP TABLE IF EXISTS carritos CASCADE;
DROP TABLE IF EXISTS productos CASCADE;
DROP TABLE IF EXISTS subcategorias CASCADE;
DROP TABLE IF EXISTS categorias CASCADE;
DROP TABLE IF EXISTS usuarios CASCADE;

-- ----------------------------------------------------------------------------
-- 1. TABLA: usuarios
-- ----------------------------------------------------------------------------
CREATE TABLE usuarios (
    id_usuario      SERIAL PRIMARY KEY,
    nombre          VARCHAR(100) NOT NULL,
    nombre_usuario  VARCHAR(50)  NOT NULL UNIQUE,
    contrasena      VARCHAR(255) NOT NULL,
    rol             VARCHAR(20)  NOT NULL CHECK (rol IN ('ADMINISTRADOR', 'MESERO', 'COCINA')),
    activo          BOOLEAN      NOT NULL DEFAULT TRUE
);

-- ----------------------------------------------------------------------------
-- 2. TABLA: categorias
-- ----------------------------------------------------------------------------
CREATE TABLE categorias (
    id_categoria    SERIAL PRIMARY KEY,
    nombre          VARCHAR(60)  NOT NULL UNIQUE,
    descripcion     VARCHAR(255) NULL
);

-- ----------------------------------------------------------------------------
-- 3. TABLA: subcategorias
-- ----------------------------------------------------------------------------
CREATE TABLE subcategorias (
    id_subcategoria SERIAL PRIMARY KEY,
    id_categoria    INT          NOT NULL,
    nombre          VARCHAR(60)  NOT NULL,
    CONSTRAINT fk_subcategoria_categoria
        FOREIGN KEY (id_categoria) REFERENCES categorias (id_categoria)
        ON DELETE CASCADE
);

-- ----------------------------------------------------------------------------
-- 4. TABLA: productos
-- ----------------------------------------------------------------------------
CREATE TABLE productos (
    id_producto     SERIAL PRIMARY KEY,
    id_categoria    INT            NOT NULL,
    id_subcategoria INT            NULL,
    nombre          VARCHAR(100)   NOT NULL,
    descripcion     TEXT           NULL,
    precio          DECIMAL(10, 2) NOT NULL CHECK (precio >= 0),
    imagen          VARCHAR(150)   NULL DEFAULT 'dotnet_bot.png',
    disponible      BOOLEAN        NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_producto_categoria
        FOREIGN KEY (id_categoria) REFERENCES categorias (id_categoria)
        ON DELETE RESTRICT,
    CONSTRAINT fk_producto_subcategoria
        FOREIGN KEY (id_subcategoria) REFERENCES subcategorias (id_subcategoria)
        ON DELETE SET NULL
);

-- ----------------------------------------------------------------------------
-- 5. TABLA: carritos (Pre-orden temporal por mesero)
-- ----------------------------------------------------------------------------
CREATE TABLE carritos (
    id_carrito      SERIAL PRIMARY KEY,
    id_mesero       INT            NOT NULL UNIQUE,
    fecha_creacion  TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    total_general   DECIMAL(10, 2) NOT NULL DEFAULT 0.00 CHECK (total_general >= 0),
    CONSTRAINT fk_carrito_mesero
        FOREIGN KEY (id_mesero) REFERENCES usuarios (id_usuario)
        ON DELETE CASCADE
);

-- ----------------------------------------------------------------------------
-- 6. TABLA: detalle_carrito
-- ----------------------------------------------------------------------------
CREATE TABLE detalle_carrito (
    id_detalle_carrito SERIAL PRIMARY KEY,
    id_carrito         INT            NOT NULL,
    id_producto        INT            NOT NULL,
    cantidad           INT            NOT NULL CHECK (cantidad > 0),
    precio_unitario    DECIMAL(10, 2) NOT NULL CHECK (precio_unitario >= 0),
    subtotal           DECIMAL(10, 2) NOT NULL CHECK (subtotal >= 0),
    CONSTRAINT fk_detalle_carrito_carrito
        FOREIGN KEY (id_carrito) REFERENCES carritos (id_carrito)
        ON DELETE CASCADE,
    CONSTRAINT fk_detalle_carrito_producto
        FOREIGN KEY (id_producto) REFERENCES productos (id_producto)
        ON DELETE CASCADE,
    CONSTRAINT uq_carrito_producto UNIQUE (id_carrito, id_producto)
);

-- ----------------------------------------------------------------------------
-- 7. TABLA: pedidos (Comandas enviadas a cocina)
-- ----------------------------------------------------------------------------
CREATE TABLE pedidos (
    id_pedido       SERIAL PRIMARY KEY,
    id_mesero       INT            NOT NULL,
    id_cocinero     INT            NULL,
    numero_mesa     INT            NOT NULL CHECK (numero_mesa > 0),
    notas           TEXT           NULL,
    estado          VARCHAR(20)    NOT NULL DEFAULT 'PENDIENTE'
                                   CHECK (estado IN ('PENDIENTE', 'PREPARANDO', 'LISTO')),
    fecha_hora      TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    total           DECIMAL(10, 2) NOT NULL DEFAULT 0.00 CHECK (total >= 0),
    CONSTRAINT fk_pedido_mesero
        FOREIGN KEY (id_mesero) REFERENCES usuarios (id_usuario)
        ON DELETE RESTRICT,
    CONSTRAINT fk_pedido_cocinero
        FOREIGN KEY (id_cocinero) REFERENCES usuarios (id_usuario)
        ON DELETE SET NULL
);

-- ----------------------------------------------------------------------------
-- 8. TABLA: detalle_pedido
-- ----------------------------------------------------------------------------
CREATE TABLE detalle_pedido (
    id_detalle_pedido SERIAL PRIMARY KEY,
    id_pedido         INT            NOT NULL,
    id_producto       INT            NOT NULL,
    cantidad          INT            NOT NULL CHECK (cantidad > 0),
    precio_unitario   DECIMAL(10, 2) NOT NULL CHECK (precio_unitario >= 0),
    subtotal          DECIMAL(10, 2) NOT NULL CHECK (subtotal >= 0),
    CONSTRAINT fk_detalle_pedido_pedido
        FOREIGN KEY (id_pedido) REFERENCES pedidos (id_pedido)
        ON DELETE CASCADE,
    CONSTRAINT fk_detalle_pedido_producto
        FOREIGN KEY (id_producto) REFERENCES productos (id_producto)
        ON DELETE RESTRICT
);

-- ----------------------------------------------------------------------------
-- ÍNDICES PARA CONSULTAS FRECUENTES
-- ----------------------------------------------------------------------------
CREATE INDEX idx_productos_categoria ON productos (id_categoria);
CREATE INDEX idx_productos_subcategoria ON productos (id_subcategoria);
CREATE INDEX idx_pedidos_estado ON pedidos (estado);
CREATE INDEX idx_pedidos_fecha ON pedidos (fecha_hora DESC);
CREATE INDEX idx_detalle_pedido_pedido ON detalle_pedido (id_pedido);