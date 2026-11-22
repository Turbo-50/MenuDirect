-- ============================================================================
-- PROYECTO: MenuDirect
-- ARCHIVO: Database/seed.sql
-- DESCRIPCIÓN: Poblado inicial con exactamente 100 registros consistentes
-- ============================================================================

BEGIN;

-- ----------------------------------------------------------------------------
-- 1. USUARIOS (8 registros)
-- ----------------------------------------------------------------------------
INSERT INTO usuarios (id_usuario, nombre, nombre_usuario, contrasena, rol, activo) VALUES
(1, 'Angel Cancino (Admin)',   'admin',   '1234', 'ADMINISTRADOR', TRUE),
(2, 'Laura Méndez Gerente',    'laura',   '1234', 'ADMINISTRADOR', TRUE),
(3, 'Carlos Hernández',        'mesero1', '1234', 'MESERO',        TRUE),
(4, 'Sofía Ramírez',           'mesero2', '1234', 'MESERO',        TRUE),
(5, 'Diego Morales',           'mesero3', '1234', 'MESERO',        TRUE),
(6, 'Valeria Castro',          'mesero4', '1234', 'MESERO',        TRUE),
(7, 'Chef Roberto Silva',      'cocina1', '1234', 'COCINA',        TRUE),
(8, 'Cocinera Elena Torres',   'cocina2', '1234', 'COCINA',        TRUE);

-- ----------------------------------------------------------------------------
-- 2. CATEGORÍAS (5 registros)
-- ----------------------------------------------------------------------------
INSERT INTO categorias (id_categoria, nombre, descripcion) VALUES
(1, 'Desayunos', 'Platillos matutinos, huevos al gusto y panadería'),
(2, 'Antojitos', 'Especialidades mexicanas tradicionales'),
(3, 'Comidas',   'Platos fuertes, cortes, guisos y caldos'),
(4, 'Postres',   'Repostería casera y postres fríos'),
(5, 'Bebidas',   'Refrescos embotellados, aguas frescas y bebidas');

-- ----------------------------------------------------------------------------
-- 3. SUBCATEGORÍAS (12 registros)
-- ----------------------------------------------------------------------------
INSERT INTO subcategorias (id_subcategoria, id_categoria, nombre) VALUES
(1,  1, 'Huevos y Chilaquiles'),
(2,  1, 'Hot Cakes y Pan Dulce'),
(3,  2, 'Burritos'),
(4,  2, 'Flautas'),
(5,  2, 'Gorditas y Empanadas'),
(6,  3, 'Cortes y Parrilla'),
(7,  3, 'Platillos Tradicionales'),
(8,  3, 'Caldos y Sopas'),
(9,  4, 'Flanes y Gelatinas'),
(10, 4, 'Pasteles y Helados'),
(11, 5, 'Embotellados'),
(12, 5, 'Aguas de Sabor');

-- ----------------------------------------------------------------------------
-- 4. PRODUCTOS (30 registros)
-- ----------------------------------------------------------------------------
INSERT INTO productos (id_producto, id_categoria, id_subcategoria, nombre, descripcion, precio, imagen, disponible) VALUES
-- Desayunos (1 - 6)
(1,  1, 1, 'Huevos motuleños',                'Tortilla frita, frijoles, jamón, chícharos y salsa',   95.00, 'dotnet_bot.png', TRUE),
(2,  1, 1, 'Huevos estrellados',              'Dos piezas acompañadas de frijoles refritos',           85.00, 'dotnet_bot.png', TRUE),
(3,  1, 1, 'Chilaquiles verdes con pollo',    'Totopos bañados en salsa verde con crema y queso',      90.00, 'dotnet_bot.png', TRUE),
(4,  1, 1, 'Chilaquiles rojos con huevo',     'Totopos en salsa roja con dos huevos al gusto',         85.00, 'dotnet_bot.png', TRUE),
(5,  1, 2, 'Hot cakes tradicionales (3 pzas)','Acompañados de mantequilla y miel de maple',            75.00, 'dotnet_bot.png', TRUE),
(6,  1, 2, 'Pan dulce artesanal con nata',    'Pieza horneada del día con nata fresca',                45.00, 'dotnet_bot.png', TRUE),
-- Antojitos (7 - 15)
(7,  2, 3, 'Burrito de asada',                'Tortilla de harina gigante con carne asada y queso',    80.00, 'dotnet_bot.png', TRUE),
(8,  2, 3, 'Burrito de pastor',               'Carne al pastor con piña, cilantro y cebolla',          75.00, 'dotnet_bot.png', TRUE),
(9,  2, 3, 'Burrito de pollo a la plancha',   'Pechuga sazonada con frijoles y guacamole',             75.00, 'dotnet_bot.png', TRUE),
(10, 2, 4, 'Flautas de pollo (4 pzas)',       'Doradas con lechuga, crema, queso y salsa',             85.00, 'dotnet_bot.png', TRUE),
(11, 2, 4, 'Flautas de papa con chorizo',     'Orden de 4 piezas crujientes con guarnición',           80.00, 'dotnet_bot.png', TRUE),
(12, 2, 5, 'Gorditas de chicharrón (2 pzas)', 'Masa artesanal rellena de chicharrón prensado',         60.00, 'dotnet_bot.png', TRUE),
(13, 2, 5, 'Empanadas de queso (3 pzas)',     'Empanadas fritas rellenas de queso manchego',           70.00, 'dotnet_bot.png', TRUE),
(14, 2, 5, 'Empanadas de carne molida',       'Orden de 3 empanadas sazonadas con especias',           75.00, 'dotnet_bot.png', TRUE),
(15, 2, 5, 'Empanadas de pollo (3 pzas)',     'Rellenas de pollo deshebrado con salsa verde',          75.00, 'dotnet_bot.png', TRUE),
-- Comidas (16 - 21)
(16, 3, 6, 'Arrachera a la parrilla (250g)',  'Corte suave con papas a la francesa y guacamole',      165.00, 'dotnet_bot.png', TRUE),
(17, 3, 6, 'Pechuga asada con guarnición',    'Pechuga de pollo con arroz y ensalada fresca',         120.00, 'dotnet_bot.png', TRUE),
(18, 3, 7, 'Enchiladas suizas',               'Rellenas de pollo gratinadas con queso manchego',      110.00, 'dotnet_bot.png', TRUE),
(19, 3, 7, 'Milanesa de res empanizada',      'Acompañada de puré de papa y ensalada',                125.00, 'dotnet_bot.png', TRUE),
(20, 3, 8, 'Caldo tlalpeño',                  'Caldo de pollo con garbanzo, chipotle y aguacate',      95.00, 'dotnet_bot.png', TRUE),
(21, 3, 8, 'Sopa de tortilla',                'Caldillo de jitomate con tiras fritas y queso',         80.00, 'dotnet_bot.png', TRUE),
-- Postres (22 - 25)
(22, 4, 9,  'Flan napolitano casero',         'Porción individual con caramelo líquido',               50.00, 'dotnet_bot.png', TRUE),
(23, 4, 9,  'Gelatina mosaico con crema',     'Cubos de sabores en base de tres leches',               40.00, 'dotnet_bot.png', TRUE),
(24, 4, 10, 'Pastel tres leches',             'Rebanada húmeda con cobertura de chantilly',            65.00, 'dotnet_bot.png', TRUE),
(25, 4, 10, 'Copa de helado napolitano',      'Tres bolas de helado de vainilla, fresa y chocolate',   55.00, 'dotnet_bot.png', TRUE),
-- Bebidas (26 - 30)
(26, 5, 11, 'Coca Cola 600 ml',               'Refresco embotellado frío',                             30.00, 'dotnet_bot.png', TRUE),
(27, 5, 11, 'Fanta Naranja 600 ml',           'Refresco sabor naranja frío',                           30.00, 'dotnet_bot.png', TRUE),
(28, 5, 11, 'Manzanita Sol 600 ml',           'Refresco sabor manzana frío',                           30.00, 'dotnet_bot.png', TRUE),
(29, 5, 12, 'Agua de jamaica 600 ml',         'Agua fresca natural preparada del día',                 25.00, 'dotnet_bot.png', TRUE),
(30, 5, 12, 'Agua de horchata 600 ml',        'Agua fresca de arroz con canela',                       25.00, 'dotnet_bot.png', TRUE);

-- ----------------------------------------------------------------------------
-- 5. CARRITOS (4 registros - 1 por cada mesero)
-- ----------------------------------------------------------------------------
INSERT INTO carritos (id_carrito, id_mesero, fecha_creacion, total_general) VALUES
(1, 3, CURRENT_TIMESTAMP - INTERVAL '15 minutes', 270.00),
(2, 4, CURRENT_TIMESTAMP - INTERVAL '10 minutes', 220.00),
(3, 5, CURRENT_TIMESTAMP,                         0.00),
(4, 6, CURRENT_TIMESTAMP,                         0.00);

-- ----------------------------------------------------------------------------
-- 6. DETALLE_CARRITO (6 registros)
-- ----------------------------------------------------------------------------
INSERT INTO detalle_carrito (id_detalle_carrito, id_carrito, id_producto, cantidad, precio_unitario, subtotal) VALUES
-- Carrito 1 (Mesero Carlos): 160 + 60 + 50 = 270.00
(1, 1, 7,  2, 80.00, 160.00),
(2, 1, 26, 2, 30.00,  60.00),
(3, 1, 22, 1, 50.00,  50.00),
-- Carrito 2 (Mesera Sofía): 95 + 50 + 75 = 220.00
(4, 2, 1,  1, 95.00,  95.00),
(5, 2, 29, 2, 25.00,  50.00),
(6, 2, 5,  1, 75.00,  75.00);

-- ----------------------------------------------------------------------------
-- 7. PEDIDOS (10 registros)
-- ----------------------------------------------------------------------------
INSERT INTO pedidos (id_pedido, id_mesero, id_cocinero, numero_mesa, notas, estado, fecha_hora, total) VALUES
(1,  3, 7,    1, 'Sin cebolla en los burritos',       'LISTO',      CURRENT_TIMESTAMP - INTERVAL '55 minutes', 325.00),
(2,  4, 7,    4, 'Salsa verde aparte',                'LISTO',      CURRENT_TIMESTAMP - INTERVAL '48 minutes', 220.00),
(3,  3, 8,    2, 'Término medio en la arrachera',     'PREPARANDO', CURRENT_TIMESTAMP - INTERVAL '32 minutes', 470.00),
(4,  5, 7,    7, 'Chilaquiles con poco picante',      'PREPARANDO', CURRENT_TIMESTAMP - INTERVAL '25 minutes', 350.00),
(5,  4, 8,    5, NULL,                                'PREPARANDO', CURRENT_TIMESTAMP - INTERVAL '20 minutes', 350.00),
(6,  6, NULL, 3, 'Sin crema en las gorditas',         'PENDIENTE',  CURRENT_TIMESTAMP - INTERVAL '14 minutes', 310.00),
(7,  3, NULL, 8, 'Hielo aparte',                      'PENDIENTE',  CURRENT_TIMESTAMP - INTERVAL '11 minutes', 310.00),
(8,  5, 8,    6, NULL,                                'LISTO',      CURRENT_TIMESTAMP - INTERVAL '40 minutes', 410.00),
(9,  6, 7,    9, 'Caldo bien caliente, sin aguacate', 'PREPARANDO', CURRENT_TIMESTAMP - INTERVAL '18 minutes', 265.00),
(10, 4, NULL, 10,'Para celebrar cumpleaños',          'PENDIENTE',  CURRENT_TIMESTAMP - INTERVAL '5 minutes',  350.00);

-- ----------------------------------------------------------------------------
-- 8. DETALLE_PEDIDO (25 registros)
-- ----------------------------------------------------------------------------
INSERT INTO detalle_pedido (id_detalle_pedido, id_pedido, id_producto, cantidad, precio_unitario, subtotal) VALUES
-- Pedido 1 (Total: 325.00)
(1,  1, 7,  2,  80.00, 160.00),
(2,  1, 8,  1,  75.00,  75.00),
(3,  1, 26, 3,  30.00,  90.00),
-- Pedido 2 (Total: 220.00)
(4,  2, 10, 2,  85.00, 170.00),
(5,  2, 30, 2,  25.00,  50.00),
-- Pedido 3 (Total: 470.00)
(6,  3, 16, 2, 165.00, 330.00),
(7,  3, 21, 1,  80.00,  80.00),
(8,  3, 26, 2,  30.00,  60.00),
-- Pedido 4 (Total: 350.00)
(9,  4, 3,  2,  90.00, 180.00),
(10, 4, 1,  1,  95.00,  95.00),
(11, 4, 29, 3,  25.00,  75.00),
-- Pedido 5 (Total: 350.00)
(12, 5, 18, 2, 110.00, 220.00),
(13, 5, 24, 2,  65.00, 130.00),
-- Pedido 6 (Total: 310.00)
(14, 6, 12, 3,  60.00, 180.00),
(15, 6, 13, 1,  70.00,  70.00),
(16, 6, 27, 2,  30.00,  60.00),
-- Pedido 7 (Total: 310.00)
(17, 7, 19, 2, 125.00, 250.00),
(18, 7, 28, 2,  30.00,  60.00),
-- Pedido 8 (Total: 410.00)
(19, 8, 2,  2,  85.00, 170.00),
(20, 8, 5,  2,  75.00, 150.00),
(21, 8, 6,  2,  45.00,  90.00),
-- Pedido 9 (Total: 265.00)
(22, 9, 20, 2,  95.00, 190.00),
(23, 9, 14, 1,  75.00,  75.00),
-- Pedido 10 (Total: 350.00)
(24, 10, 17, 2, 120.00, 240.00),
(25, 10, 25, 2,  55.00, 110.00);

-- ----------------------------------------------------------------------------
-- SINCRONIZACIÓN DE SECUENCIAS (Evita errores de PK duplicada en nuevos INSERT)
-- ----------------------------------------------------------------------------
SELECT setval('usuarios_id_usuario_seq',               (SELECT MAX(id_usuario) FROM usuarios));
SELECT setval('categorias_id_categoria_seq',           (SELECT MAX(id_categoria) FROM categorias));
SELECT setval('subcategorias_id_subcategoria_seq',     (SELECT MAX(id_subcategoria) FROM subcategorias));
SELECT setval('productos_id_producto_seq',             (SELECT MAX(id_producto) FROM productos));
SELECT setval('carritos_id_carrito_seq',               (SELECT MAX(id_carrito) FROM carritos));
SELECT setval('detalle_carrito_id_detalle_carrito_seq',(SELECT MAX(id_detalle_carrito) FROM detalle_carrito));
SELECT setval('pedidos_id_pedido_seq',                 (SELECT MAX(id_pedido) FROM pedidos));
SELECT setval('detalle_pedido_id_detalle_pedido_seq',  (SELECT MAX(id_detalle_pedido) FROM detalle_pedido));

COMMIT;