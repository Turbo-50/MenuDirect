// =========================================================================
// 1. CONFIGURACIÓN Y ESTADO GLOBAL (API BACKEND)
// =========================================================================
const API_URL = (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1')
  ? 'http://localhost:3000/api'
  : '/api';

let productos = [];
let pedidos = [];
let usuariosRegistrados = [];
let usuarioActual = JSON.parse(localStorage.getItem('md_usuario')) || null;

const carrito = {};
const IMAGEN_GENERICA = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500&auto=format&fit=crop&q=60";

// Sincronizar datos iniciales desde el servidor PostgreSQL
async function cargarDatosBackend() {
  try {
    const [resProd, resPed] = await Promise.all([
      fetch(`${API_URL}/productos`),
      fetch(`${API_URL}/pedidos`)
    ]);

    productos = await resProd.json();
    pedidos = await resPed.json();
  } catch (err) {
    console.error("Error al conectar con el servidor backend:", err);
  }
}

// =========================================================================
// 2. INICIALIZACIÓN DE LA APLICACIÓN
// =========================================================================
document.addEventListener('DOMContentLoaded', async () => {
  await cargarDatosBackend();
  renderizarProductos(productos);
  configurarNavegacionYFiltros();
  configurarCarritoYCheckout();
  configurarAutenticacion();
  configurarPerfilCliente();
  configurarPanelAdminYProductos();
  actualizarBotonHeaderUsuario();
});

function actualizarBotonHeaderUsuario() {
  const btnLogin = document.getElementById('btnLogin');
  if (!btnLogin) return;

  if (usuarioActual) {
    btnLogin.textContent = (usuarioActual.rol === 'sucursal')
      ? `🏢 ${usuarioActual.sucursal}`
      : "👤 Mi Cuenta";
  } else {
    btnLogin.textContent = "Iniciar sesión";
  }
}

// =========================================================================
// 3. CATÁLOGO Y FILTROS
// =========================================================================
function renderizarProductos(lista) {
  const productsContainer = document.getElementById('productsContainer');
  if (!productsContainer) return;
  productsContainer.innerHTML = '';

  const visibles = lista.filter(p => p.activo);
  if (visibles.length === 0) {
    productsContainer.innerHTML = `<p class="no-results">No se encontraron productos disponibles.</p>`;
    return;
  }

  visibles.forEach(prod => {
    const cantidadActual = carrito[prod.id] || 0;
    const badgeClass = prod.esCadena ? 'badge-chain' : 'badge-local';
    const badgeIcon = prod.esCadena ? '🏢' : '🏪';
    const direccionTexto = prod.sucursalDireccion && prod.sucursalDireccion.trim() !== ''
      ? `📍 ${prod.sucursalDireccion}`
      : '📍 Dirección no disponible';

    const card = document.createElement('article');
    card.className = 'product-card';
    card.innerHTML = `
      <div class="product-image-wrap">
        <img src="${prod.imagen || IMAGEN_GENERICA}" alt="${prod.nombre}" loading="lazy">
        <span class="branch-badge ${badgeClass}">${badgeIcon} ${prod.sucursal}</span>
      </div>
      <div class="product-info">
        <h3 class="product-title">${prod.nombre}</h3>
        <p class="product-address" style="font-size: 0.8rem; color: var(--text-muted); margin-top: -2px;">${direccionTexto}</p>
        <p class="product-price">$${parseFloat(prod.precio).toFixed(2)}</p>
        <div class="quantity-controls">
          <button type="button" class="btn-qty minus" data-id="${prod.id}">-</button>
          <div class="qty-display">
            <span class="qty-icon">🛒</span>
            <span class="qty-num" id="qty-${prod.id}">${cantidadActual}</span>
          </div>
          <button type="button" class="btn-qty plus" data-id="${prod.id}">+</button>
        </div>
      </div>
    `;
    productsContainer.appendChild(card);
  });

  document.querySelectorAll('.btn-qty.plus').forEach(btn => {
    btn.onclick = (e) => modificarCantidad(parseInt(e.target.dataset.id), 1);
  });
  document.querySelectorAll('.btn-qty.minus').forEach(btn => {
    btn.onclick = (e) => modificarCantidad(parseInt(e.target.dataset.id), -1);
  });
}

function configurarNavegacionYFiltros() {
  const logoBtn = document.querySelector('.logo');
  const searchInput = document.getElementById('searchInput');
  const categoryButtons = document.querySelectorAll('.category-card');

  const mainHeader = document.querySelector('.main-header');
  const btnToggleSearch = document.getElementById('btnToggleSearch');
  const btnCloseSearch = document.getElementById('btnCloseSearch');

  if (btnToggleSearch && mainHeader && searchInput) {
    btnToggleSearch.onclick = () => {
      mainHeader.classList.add('search-expanded');
      setTimeout(() => searchInput.focus(), 50);
    };
  }

  if (btnCloseSearch && mainHeader && searchInput) {
    btnCloseSearch.onclick = () => {
      mainHeader.classList.remove('search-expanded');
      searchInput.value = '';
      renderizarProductos(productos);
    };
  }

  if (logoBtn) {
    logoBtn.style.cursor = 'pointer';
    logoBtn.onclick = () => {
      if (searchInput) searchInput.value = '';
      categoryButtons.forEach(b => b.classList.remove('active'));
      const btnTodos = document.querySelector('.category-card[data-category="todos"]');
      if (btnTodos) btnTodos.classList.add('active');
      renderizarProductos(productos);
    };
  }

  categoryButtons.forEach(btn => {
    btn.onclick = () => {
      categoryButtons.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');

      const cat = btn.dataset.category;
      if (cat === 'todos') {
        renderizarProductos(productos);
      } else {
        renderizarProductos(productos.filter(p => p.categoria.toLowerCase() === cat.toLowerCase()));
      }
    };
  });

  if (searchInput) {
    searchInput.oninput = (e) => {
      const texto = e.target.value.toLowerCase().trim();
      renderizarProductos(productos.filter(p => 
        p.nombre.toLowerCase().includes(texto) || 
        p.sucursal.toLowerCase().includes(texto) ||
        (p.sucursalDireccion && p.sucursalDireccion.toLowerCase().includes(texto)) ||
        p.categoria.toLowerCase().includes(texto)
      ));
    };
  }
}

// =========================================================================
// 4. CARRITO Y CHECKOUT
// =========================================================================
function modificarCantidad(id, cambio) {
  const cantidadActual = carrito[id] || 0;
  const nuevaCantidad = cantidadActual + cambio;

  if (nuevaCantidad <= 0) {
    delete carrito[id];
  } else {
    carrito[id] = nuevaCantidad;
  }

  const qtyElem = document.getElementById(`qty-${id}`);
  if (qtyElem) qtyElem.textContent = carrito[id] || 0;

  const cartBadge = document.getElementById('cartCount');
  const totalItems = Object.values(carrito).reduce((acc, qty) => acc + qty, 0);
  if (cartBadge) cartBadge.textContent = totalItems;

  const cartModal = document.getElementById('cartModal');
  if (cartModal && cartModal.classList.contains('active')) {
    renderizarModalCarrito();
  }
}

function renderizarModalCarrito() {
  const cartItemsContainer = document.getElementById('cartItemsContainer');
  const cartTotal = document.getElementById('cartTotal');
  const btnGoToCheckout = document.getElementById('btnGoToCheckout');

  if (!cartItemsContainer) return;
  cartItemsContainer.innerHTML = '';
  const itemIds = Object.keys(carrito);

  if (itemIds.length === 0) {
    cartItemsContainer.innerHTML = `<p class="empty-cart-msg">Tu carrito está vacío.</p>`;
    if (cartTotal) cartTotal.textContent = '$0.00';
    if (btnGoToCheckout) btnGoToCheckout.disabled = true;
    return;
  }

  let totalGeneral = 0;

  itemIds.forEach(id => {
    const prod = productos.find(p => p.id === parseInt(id));
    if (!prod) return;
    const cant = carrito[id];
    const subtotalItem = parseFloat(prod.precio) * cant;
    totalGeneral += subtotalItem;

    const row = document.createElement('div');
    row.className = 'cart-item-row';
    row.innerHTML = `
      <img src="${prod.imagen || IMAGEN_GENERICA}" alt="${prod.nombre}" class="cart-item-img">
      <div class="cart-item-details">
        <h4>${prod.nombre}</h4>
        <span class="cart-item-branch">${prod.sucursal} ${prod.sucursalDireccion ? `(${prod.sucursalDireccion})` : ''}</span>
        <p class="cart-item-price">$${parseFloat(prod.precio).toFixed(2)} c/u</p>
      </div>
      <div class="cart-item-actions">
        <div class="quantity-controls mini">
          <button type="button" class="btn-qty minus" data-id="${prod.id}">-</button>
          <span>${cant}</span>
          <button type="button" class="btn-qty plus" data-id="${prod.id}">+</button>
        </div>
        <span class="cart-item-subtotal">$${subtotalItem.toFixed(2)}</span>
      </div>
    `;

    row.querySelector('.btn-qty.plus').onclick = () => modificarCantidad(prod.id, 1);
    row.querySelector('.btn-qty.minus').onclick = () => modificarCantidad(prod.id, -1);

    cartItemsContainer.appendChild(row);
  });

  if (cartTotal) cartTotal.textContent = `$${totalGeneral.toFixed(2)}`;
  if (btnGoToCheckout) btnGoToCheckout.disabled = false;
}

function configurarCarritoYCheckout() {
  const cartButton = document.getElementById('cartButton');
  const cartModal = document.getElementById('cartModal');
  const btnCloseCart = document.getElementById('btnCloseCart');
  const btnGoToCheckout = document.getElementById('btnGoToCheckout');
  const btnBackToCart = document.getElementById('btnBackToCart');
  const payCashRadio = document.getElementById('payCash');
  const payCardRadio = document.getElementById('payCard');
  const cashDetails = document.getElementById('cashDetails');
  const checkoutForm = document.getElementById('checkoutForm');
  const btnCloseSuccess = document.getElementById('btnCloseSuccess');

  if (cartButton) {
    cartButton.onclick = (e) => {
      e.stopPropagation();
      renderizarModalCarrito();
      cambiarVistaCarrito('cart');
      if (cartModal) cartModal.classList.add('active');
    };
  }

  if (btnCloseCart) btnCloseCart.onclick = () => cartModal && cartModal.classList.remove('active');
  if (cartModal) {
    cartModal.onclick = (e) => {
      if (e.target === cartModal) cartModal.classList.remove('active');
    };
  }

if (btnGoToCheckout) {
    btnGoToCheckout.onclick = () => {
      if (usuarioActual && usuarioActual.rol === 'cliente') {
        if (usuarioActual.nombre) document.getElementById('inputNombre').value = usuarioActual.nombre;
        if (usuarioActual.direccion) document.getElementById('inputDireccion').value = usuarioActual.direccion;
        
        // Precargar el método de pago predeterminado del usuario
        const metodo = (usuarioActual.metodoPago || '').toLowerCase();
        if (metodo.includes('tarjeta')) {
          if (payCardRadio) {
            payCardRadio.checked = true;
            if (cashDetails) cashDetails.classList.add('hidden');
          }
        } else {
          if (payCashRadio) {
            payCashRadio.checked = true;
            if (cashDetails) cashDetails.classList.remove('hidden');
          }
        }
      }
      cambiarVistaCarrito('checkout');
    };
  }

  if (btnBackToCart) btnBackToCart.onclick = () => cambiarVistaCarrito('cart');
  if (payCashRadio) payCashRadio.onchange = () => cashDetails && cashDetails.classList.remove('hidden');
  if (payCardRadio) payCardRadio.onchange = () => cashDetails && cashDetails.classList.add('hidden');

  if (checkoutForm) {
    checkoutForm.onsubmit = async (e) => {
      e.preventDefault();
      const numOrden = Math.floor(1000 + Math.random() * 9000);
      const idOrden = `MD-${numOrden}`;
      const successOrderNum = document.getElementById('successOrderNum');
      if (successOrderNum) successOrderNum.textContent = `Número de orden: #${idOrden}`;

      const itemsComprados = Object.keys(carrito).map(id => {
        const p = productos.find(x => x.id === parseInt(id));
        return { 
          idProducto: p.id,
          nombre: p.nombre, 
          sucursal: p.sucursal, 
          cantidad: carrito[id], 
          subtotal: parseFloat(p.precio) * carrito[id],
          estado: "Recibido" 
        };
      });

      const totalCompra = itemsComprados.reduce((a, b) => a + b.subtotal, 0);

      const nuevoPedidoPayload = {
        id: idOrden,
        cliente: document.getElementById('inputNombre').value || "Cliente General",
        telefono: document.getElementById('inputTelefono').value || "S/T",
        direccion: document.getElementById('inputDireccion').value || "Entrega en domicilio",
        metodoPago: payCashRadio.checked ? "Efectivo" : "Tarjeta",
        totalGeneral: totalCompra,
        detalles: itemsComprados
      };

      try {
        const response = await fetch(`${API_URL}/pedidos`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(nuevoPedidoPayload)
        });

        if (response.ok) {
          pedidos.unshift(nuevoPedidoPayload);
          Object.keys(carrito).forEach(id => delete carrito[id]);
          const cartBadge = document.getElementById('cartCount');
          if (cartBadge) cartBadge.textContent = 0;
          renderizarProductos(productos);
          cambiarVistaCarrito('success');
        } else {
          alert("Error al registrar el pedido en la base de datos.");
        }
      } catch (err) {
        console.error("Error de red al enviar pedido:", err);
      }
    };
  }

  if (btnCloseSuccess) {
    btnCloseSuccess.onclick = () => {
      if (cartModal) cartModal.classList.remove('active');
      cambiarVistaCarrito('cart');
    };
  }
}

function cambiarVistaCarrito(vista) {
  const cartView = document.getElementById('cartView');
  const checkoutView = document.getElementById('checkoutView');
  const successView = document.getElementById('successView');

  if (cartView) cartView.classList.add('hidden');
  if (checkoutView) checkoutView.classList.add('hidden');
  if (successView) successView.classList.add('hidden');

  if (vista === 'cart' && cartView) cartView.classList.remove('hidden');
  if (vista === 'checkout' && checkoutView) checkoutView.classList.remove('hidden');
  if (vista === 'success' && successView) successView.classList.remove('hidden');
}

// =========================================================================
// 5. AUTENTICACIÓN Y VER CONTRASEÑA
// =========================================================================
function validarPassword(password) {
  const minLongitud = password.length >= 8;
  const tieneNumero = /[0-9]/.test(password);
  const tieneEspecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);
  return { valido: minLongitud && tieneNumero && tieneEspecial, minLongitud, tieneNumero, tieneEspecial };
}

function mostrarSubvistaAuth(vista) {
  const views = {
    login: document.getElementById('authLoginView'),
    register: document.getElementById('authRegisterView'),
    forgot: document.getElementById('authForgotView')
  };
  const tabBtnLogin = document.getElementById('tabBtnLogin');
  const tabBtnRegister = document.getElementById('tabBtnRegister');

  Object.values(views).forEach(v => v && v.classList.add('hidden'));
  if (tabBtnLogin) tabBtnLogin.classList.remove('active');
  if (tabBtnRegister) tabBtnRegister.classList.remove('active');

  if (views[vista]) views[vista].classList.remove('hidden');
  if (vista === 'login' && tabBtnLogin) tabBtnLogin.classList.add('active');
  if (vista === 'register' && tabBtnRegister) tabBtnRegister.classList.add('active');
}

function configurarAutenticacion() {
  const btnLogin = document.getElementById('btnLogin');
  const loginModal = document.getElementById('loginModal');
  const btnCloseLogin = document.getElementById('btnCloseLogin');
  const tabBtnLogin = document.getElementById('tabBtnLogin');
  const tabBtnRegister = document.getElementById('tabBtnRegister');
  const btnForgotPassword = document.getElementById('btnForgotPassword');
  const btnBackToLoginFromForgot = document.getElementById('btnBackToLoginFromForgot');

  const loginForm = document.getElementById('loginForm');
  const registerForm = document.getElementById('registerForm');
  const forgotForm = document.getElementById('forgotForm');

  const regRoleSelect = document.getElementById('regRole');
  const lblRegName = document.getElementById('lblRegName');
  const regNameInput = document.getElementById('regName');
  const branchExtraFields = document.getElementById('branchExtraFields');
  const regAddressInput = document.getElementById('regAddress');
  const regPhoneInput = document.getElementById('regPhone');
  const regPasswordInput = document.getElementById('regPassword');
  const passHint = document.getElementById('passHint');

  const toggleLoginPass = document.getElementById('toggleLoginPassword');
  const loginPasswordInput = document.getElementById('loginPassword');
  if (toggleLoginPass && loginPasswordInput) {
    toggleLoginPass.onclick = () => {
      const tipo = loginPasswordInput.type === 'password' ? 'text' : 'password';
      loginPasswordInput.type = tipo;
      toggleLoginPass.textContent = tipo === 'password' ? '👁️' : '🔒';
    };
  }

  const toggleRegPass = document.getElementById('toggleRegPassword');
  if (toggleRegPass && regPasswordInput) {
    toggleRegPass.onclick = () => {
      const tipo = regPasswordInput.type === 'password' ? 'text' : 'password';
      regPasswordInput.type = tipo;
      toggleRegPass.textContent = tipo === 'password' ? '👁️' : '🔒';
    };
  }

  if (btnLogin) {
    btnLogin.onclick = () => {
      if (usuarioActual) {
        if (usuarioActual.rol === 'sucursal') {
          abrirPanelAdmin();
        } else {
          abrirModalPerfilCliente();
        }
      } else {
        mostrarSubvistaAuth('login');
        if (loginModal) loginModal.classList.add('active');
      }
    };
  }

  if (btnCloseLogin) btnCloseLogin.onclick = () => loginModal && loginModal.classList.remove('active');
  if (tabBtnLogin) tabBtnLogin.onclick = () => mostrarSubvistaAuth('login');
  if (tabBtnRegister) tabBtnRegister.onclick = () => mostrarSubvistaAuth('register');
  if (btnForgotPassword) btnForgotPassword.onclick = () => mostrarSubvistaAuth('forgot');
  if (btnBackToLoginFromForgot) btnBackToLoginFromForgot.onclick = () => mostrarSubvistaAuth('login');

  if (regRoleSelect) {
    regRoleSelect.onchange = () => {
      if (regRoleSelect.value === 'sucursal') {
        if (lblRegName) lblRegName.textContent = 'Nombre de la Sucursal / Marca *';
        if (regNameInput) regNameInput.placeholder = "Ej. Pizza Hut Centro";
        if (branchExtraFields) branchExtraFields.classList.remove('hidden');
        if (regAddressInput) regAddressInput.required = true;
        if (regPhoneInput) regPhoneInput.required = true;
      } else {
        if (lblRegName) lblRegName.textContent = 'Nombre completo *';
        if (regNameInput) regNameInput.placeholder = 'Ej. Juan Pérez';
        if (branchExtraFields) branchExtraFields.classList.add('hidden');
        if (regAddressInput) regAddressInput.required = false;
        if (regPhoneInput) regPhoneInput.required = false;
      }
    };
  }

  if (regPasswordInput && passHint) {
    regPasswordInput.oninput = () => {
      const val = regPasswordInput.value;
      const res = validarPassword(val);
      if (val.length === 0) {
        passHint.style.color = 'var(--text-muted)';
        passHint.textContent = 'La contraseña requiere: mínimo 8 caracteres, al menos un número y un carácter especial.';
      } else if (res.valido) {
        passHint.style.color = 'var(--secondary-green)';
        passHint.textContent = '✔ Contraseña segura y válida.';
      } else {
        passHint.style.color = '#c62828';
        passHint.textContent = 'Faltan requisitos de seguridad.';
      }
    };
  }

  // Registro de usuario en Neon
  if (registerForm) {
    registerForm.onsubmit = async (e) => {
      e.preventDefault();
      const rol = regRoleSelect.value;
      const nombre = regNameInput.value.trim();
      const correo = document.getElementById('regEmail').value.toLowerCase().trim();
      const pass = regPasswordInput.value;

      const res = validarPassword(pass);
      if (!res.valido) {
        alert("La contraseña no cumple con los requisitos mínimos.");
        return;
      }

      const payload = {
        correo,
        password: pass,
        nombre,
        rol,
        sucursal: rol === 'sucursal' ? nombre : null,
        direccion: regAddressInput ? regAddressInput.value.trim() : '',
        telefono: regPhoneInput ? regPhoneInput.value.trim() : '',
        esCadena: document.getElementById('regChainType')?.value === 'cadena'
      };

      try {
        const response = await fetch(`${API_URL}/usuarios`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });

        if (response.ok) {
          usuarioActual = payload;
          localStorage.setItem('md_usuario', JSON.stringify(usuarioActual));
          await cargarDatosBackend();

          registerForm.reset();
          if (branchExtraFields) branchExtraFields.classList.add('hidden');
          if (loginModal) loginModal.classList.remove('active');
          actualizarBotonHeaderUsuario();

          if (rol === 'sucursal') {
            abrirPanelAdmin();
          } else {
            alert(`¡Cuenta creada con éxito! Bienvenido, ${nombre}.`);
          }
        } else {
          alert("Error al registrar el usuario en la base de datos.");
        }
      } catch (err) {
        console.error("Error en registro:", err);
      }
    };
  }

// Login seguro contra /api/login
  if (loginForm) {
    loginForm.onsubmit = async (e) => {
      e.preventDefault();
      const rol = document.getElementById('loginRole').value;
      const correo = document.getElementById('loginEmail').value.toLowerCase().trim();
      const pass = document.getElementById('loginPassword').value;

      try {
        const res = await fetch(`${API_URL}/login`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ correo, password: pass, rol })
        });
        const data = await res.json();

        if (!res.ok) {
          alert(data.error || "Credenciales incorrectas.");
          return;
        }

        usuarioActual = data;
        localStorage.setItem('md_usuario', JSON.stringify(usuarioActual));

        loginForm.reset();
        if (loginModal) loginModal.classList.remove('active');
        actualizarBotonHeaderUsuario();

        if (usuarioActual.rol === 'sucursal') {
          abrirPanelAdmin();
        } else {
          alert(`¡Bienvenido de nuevo, ${usuarioActual.nombre}!`);
        }
      } catch (err) {
        console.error("Error en login:", err);
      }
    };
  }

  // Recuperación Paso 1: Enviar código por correo
  const resetForm = document.getElementById('resetForm');
  if (forgotForm) {
    forgotForm.onsubmit = async (e) => {
      e.preventDefault();
      const correo = document.getElementById('forgotEmail').value.toLowerCase().trim();
      const btnSend = document.getElementById('btnSendResetCode');
      if (btnSend) {
        btnSend.disabled = true;
        btnSend.textContent = 'Enviando código...';
      }

      try {
        const res = await fetch(`${API_URL}/auth/forgot-password`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ correo })
        });
        const data = await res.json();

        if (!res.ok) {
          alert(data.error || "No se pudo procesar la solicitud.");
        } else {
          forgotForm.classList.add('hidden');
          if (resetForm) resetForm.classList.remove('hidden');
        }
      } catch (err) {
        alert("Error de conexión al solicitar el código.");
      } finally {
        if (btnSend) {
          btnSend.disabled = false;
          btnSend.textContent = 'Enviar código al correo';
        }
      }
    };
  }

  // Recuperación Paso 2: Confirmar código y actualizar contraseña
  if (resetForm) {
    resetForm.onsubmit = async (e) => {
      e.preventDefault();
      const correo = document.getElementById('forgotEmail').value.toLowerCase().trim();
      const codigo = document.getElementById('resetCode').value.trim();
      const nuevaPassword = document.getElementById('resetNewPassword').value;

      const validacion = validarPassword(nuevaPassword);
      if (!validacion.valido) {
        alert("La nueva contraseña debe tener mínimo 8 caracteres, al menos un número y un carácter especial.");
        return;
      }

      try {
        const res = await fetch(`${API_URL}/auth/reset-password`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ correo, codigo, nuevaPassword })
        });
        const data = await res.json();

        if (!res.ok) {
          alert(data.error || "Código inválido.");
          return;
        }

        alert("✔ Contraseña actualizada correctamente. Ya puedes iniciar sesión.");
        forgotForm.reset();
        resetForm.reset();
        resetForm.classList.add('hidden');
        forgotForm.classList.remove('hidden');
        mostrarSubvistaAuth('login');
      } catch (err) {
        alert("Error al actualizar contraseña.");
      }
    };
  }
}

function cerrarSesionCompleta() {
  usuarioActual = null;
  localStorage.removeItem('md_usuario');
  actualizarBotonHeaderUsuario();
  const adminPanel = document.getElementById('adminPanel');
  if (adminPanel) adminPanel.classList.add('hidden');
}

// =========================================================================
// 6. PERFIL DE CLIENTE
// =========================================================================
function abrirModalPerfilCliente() {
  const profileModal = document.getElementById('profileModal');
  if (!profileModal || !usuarioActual) return;

  document.getElementById('profileName').value = usuarioActual.nombre || '';
  document.getElementById('profileEmail').value = usuarioActual.correo || '';
  document.getElementById('profileAddress').value = usuarioActual.direccion || '';
  document.getElementById('profilePayment').value = usuarioActual.metodoPago || 'Efectivo';

  profileModal.classList.add('active');
}

function configurarPerfilCliente() {
  const profileModal = document.getElementById('profileModal');
  const btnCloseProfile = document.getElementById('btnCloseProfile');
  const profileForm = document.getElementById('profileForm');
  const btnLogoutFromProfile = document.getElementById('btnLogoutFromProfile');

  if (btnCloseProfile) btnCloseProfile.onclick = () => profileModal && profileModal.classList.remove('active');
  if (profileModal) {
    profileModal.onclick = (e) => {
      if (e.target === profileModal) profileModal.classList.remove('active');
    };
  }

  if (profileForm) {
    profileForm.onsubmit = async (e) => {
      e.preventDefault();
      if (!usuarioActual) return;

      usuarioActual.nombre = document.getElementById('profileName').value.trim();
      usuarioActual.direccion = document.getElementById('profileAddress').value.trim();
      usuarioActual.metodoPago = document.getElementById('profilePayment').value;

      try {
        await fetch(`${API_URL}/usuarios/perfil`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(usuarioActual)
        });

        localStorage.setItem('md_usuario', JSON.stringify(usuarioActual));
        if (profileModal) profileModal.classList.remove('active');
        alert("Tus datos de perfil han sido actualizados.");
      } catch (err) {
        console.error("Error al actualizar perfil:", err);
      }
    };
  }

  if (btnLogoutFromProfile) {
    btnLogoutFromProfile.onclick = () => {
      if (profileModal) profileModal.classList.remove('active');
      cerrarSesionCompleta();
    };
  }
}

// =========================================================================
// 7. PANEL DE ADMINISTRACIÓN Y GESTIÓN DE PRODUCTOS / PEDIDOS
// =========================================================================
function abrirPanelAdmin() {
  const adminPanel = document.getElementById('adminPanel');
  const adminBranchName = document.getElementById('adminBranchName');
  const adminTabs = document.querySelectorAll('.admin-nav-tabs .tab-btn');
  const tabOrders = document.getElementById('tabOrders');
  const tabCatalog = document.getElementById('tabCatalog');

  if (!adminPanel) return;
  if (adminBranchName) adminBranchName.textContent = usuarioActual?.sucursal || "Sucursal";

  adminTabs.forEach(b => b.classList.remove('active'));
  const defaultTab = document.querySelector('.admin-nav-tabs .tab-btn[data-tab="orders"]');
  if (defaultTab) defaultTab.classList.add('active');

  if (tabOrders) tabOrders.classList.remove('hidden');
  if (tabCatalog) tabCatalog.classList.add('hidden');

  renderizarTableroPedidos();
  renderizarTablaProductosAdmin();

  adminPanel.classList.remove('hidden');
}

function configurarPanelAdminYProductos() {
  const btnExitAdmin = document.getElementById('btnExitAdmin');
  const adminTabs = document.querySelectorAll('.admin-nav-tabs .tab-btn');
  const tabOrders = document.getElementById('tabOrders');
  const tabCatalog = document.getElementById('tabCatalog');

  const btnNewProduct = document.getElementById('btnNewProduct');
  const productModal = document.getElementById('productModal');
  const btnCloseProductModal = document.getElementById('btnCloseProductModal');
  const productForm = document.getElementById('productForm');
  const btnSkipImage = document.getElementById('btnSkipImage');
  const prodModalCategory = document.getElementById('prodModalCategory');
  const customCategoryGroup = document.getElementById('customCategoryGroup');
  const prodModalCustomCategory = document.getElementById('prodModalCustomCategory');

  if (btnExitAdmin) btnExitAdmin.onclick = () => cerrarSesionCompleta();

  adminTabs.forEach(btn => {
    btn.onclick = () => {
      adminTabs.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');

      const tab = btn.dataset.tab;
      if (tab === 'orders') {
        if (tabOrders) tabOrders.classList.remove('hidden');
        if (tabCatalog) tabCatalog.classList.add('hidden');
        renderizarTableroPedidos();
      } else {
        if (tabOrders) tabOrders.classList.add('hidden');
        if (tabCatalog) tabCatalog.classList.remove('hidden');
        renderizarTablaProductosAdmin();
      }
    };
  });

  if (btnNewProduct) {
    btnNewProduct.onclick = () => {
      if (productForm) productForm.reset();
      document.getElementById('prodModalId').value = '';
      document.getElementById('productModalTitle').textContent = 'Nuevo Producto';
      if (customCategoryGroup) customCategoryGroup.classList.add('hidden');
      if (prodModalCustomCategory) prodModalCustomCategory.required = false;
      if (btnSkipImage) btnSkipImage.classList.remove('hidden');
      if (productModal) productModal.classList.add('active');
    };
  }

  if (btnCloseProductModal) btnCloseProductModal.onclick = () => productModal && productModal.classList.remove('active');
  if (productModal) {
    productModal.onclick = (e) => {
      if (e.target === productModal) productModal.classList.remove('active');
    };
  }

  if (prodModalCategory && customCategoryGroup) {
    prodModalCategory.onchange = () => {
      if (prodModalCategory.value === 'otro') {
        customCategoryGroup.classList.remove('hidden');
        if (prodModalCustomCategory) prodModalCustomCategory.required = true;
      } else {
        customCategoryGroup.classList.add('hidden');
        if (prodModalCustomCategory) {
          prodModalCustomCategory.required = false;
          prodModalCustomCategory.value = '';
        }
      }
    };
  }

  if (btnSkipImage) {
    btnSkipImage.onclick = () => {
      const nombre = document.getElementById('prodModalName').value.trim();
      const precio = parseFloat(document.getElementById('prodModalPrice').value);
      if (!nombre || isNaN(precio)) {
        alert("Por favor escribe el nombre y precio antes de omitir la imagen.");
        return;
      }
      guardarProductoEnCatalogo(IMAGEN_GENERICA);
    };
  }

  if (productForm) {
    productForm.onsubmit = (e) => {
      e.preventDefault();
      const fileInput = document.getElementById('prodModalImageFile');
      const urlInput = document.getElementById('prodModalImageUrl').value;

      if (fileInput && fileInput.files && fileInput.files[0]) {
        const reader = new FileReader();
        reader.onload = (event) => guardarProductoEnCatalogo(event.target.result);
        reader.readAsDataURL(fileInput.files[0]);
      } else if (urlInput && urlInput.trim() !== '') {
        guardarProductoEnCatalogo(urlInput.trim());
      } else {
        const idEdicion = document.getElementById('prodModalId').value;
        if (idEdicion) {
          const prodExistente = productos.find(p => p.id === parseInt(idEdicion));
          guardarProductoEnCatalogo(prodExistente ? prodExistente.imagen : IMAGEN_GENERICA);
        } else {
          guardarProductoEnCatalogo(IMAGEN_GENERICA);
        }
      }
    };
  }
}

async function guardarProductoEnCatalogo(imagenUrl) {
  if (!usuarioActual || !usuarioActual.sucursal) return;

  const idEdicion = document.getElementById('prodModalId').value;
  const nombre = document.getElementById('prodModalName').value.trim();
  const precio = parseFloat(document.getElementById('prodModalPrice').value);

  const selectCat = document.getElementById('prodModalCategory').value;
  const customCat = document.getElementById('prodModalCustomCategory').value.trim().toLowerCase();
  const categoria = (selectCat === 'otro' && customCat !== '') ? customCat : selectCat;

  const payload = {
    nombre,
    precio,
    categoria,
    sucursal: usuarioActual.sucursal,
    esCadena: usuarioActual.esCadena || false,
    imagen: imagenUrl,
    activo: true
  };

  try {
    if (idEdicion) {
      await fetch(`${API_URL}/productos/${idEdicion}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    } else {
      await fetch(`${API_URL}/productos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
    }

    await cargarDatosBackend();
    const productModal = document.getElementById('productModal');
    if (productModal) productModal.classList.remove('active');
    renderizarTablaProductosAdmin();
    renderizarProductos(productos);
  } catch (err) {
    console.error("Error al guardar producto:", err);
  }
}

function renderizarTablaProductosAdmin() {
  const adminProductsTable = document.getElementById('adminProductsTable');
  if (!adminProductsTable) return;
  adminProductsTable.innerHTML = '';

  if (!usuarioActual || !usuarioActual.sucursal) return;

  const sucursalAdmin = usuarioActual.sucursal.trim().toLowerCase();
  const misProductos = productos.filter(p => p.sucursal && p.sucursal.trim().toLowerCase() === sucursalAdmin);

  if (misProductos.length === 0) {
    adminProductsTable.innerHTML = `<tr><td colspan="6" style="text-align:center; padding: 1.5rem; color: var(--text-muted);">Aún no tienes productos registrados en <strong>${usuarioActual.sucursal}</strong>.</td></tr>`;
    return;
  }

  misProductos.forEach(prod => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><img src="${prod.imagen || IMAGEN_GENERICA}" width="45" height="45" style="object-fit:cover; border-radius:6px;"></td>
      <td><strong>${prod.nombre}</strong></td>
      <td style="text-transform: capitalize;">${prod.categoria}</td>
      <td>$${parseFloat(prod.precio).toFixed(2)}</td>
      <td><span class="status-badge active">Publicado</span></td>
      <td>
        <div style="display: flex; gap: 0.4rem;">
          <button type="button" class="btn-sm" style="background-color: #1976d2; color: white; border: none; padding: 5px 10px; border-radius: 4px; cursor: pointer;" onclick="editarProductoAdmin(${prod.id})">✏️ Editar</button>
          <button type="button" class="btn-sm" style="background-color: #d32f2f; color: white; border: none; padding: 5px 10px; border-radius: 4px; cursor: pointer;" onclick="borrarProductoAdmin(${prod.id})">🗑️ Borrar</button>
        </div>
      </td>
    `;
    adminProductsTable.appendChild(tr);
  });
}

window.editarProductoAdmin = function(id) {
  const prod = productos.find(p => p.id === id);
  if (!prod) return;

  document.getElementById('prodModalId').value = prod.id;
  document.getElementById('productModalTitle').textContent = 'Editar Producto';
  document.getElementById('prodModalName').value = prod.nombre;
  document.getElementById('prodModalPrice').value = prod.precio;

  const categoriasBase = ['pizzas', 'hamburguesas', 'tacos', 'sushi'];
  const selectCat = document.getElementById('prodModalCategory');
  const customGroup = document.getElementById('customCategoryGroup');
  const customInput = document.getElementById('prodModalCustomCategory');

  if (categoriasBase.includes(prod.categoria.toLowerCase())) {
    selectCat.value = prod.categoria.toLowerCase();
    if (customGroup) customGroup.classList.add('hidden');
    if (customInput) customInput.required = false;
  } else {
    selectCat.value = 'otro';
    if (customGroup) customGroup.classList.remove('hidden');
    if (customInput) {
      customInput.value = prod.categoria;
      customInput.required = true;
    }
  }

  document.getElementById('prodModalImageUrl').value = prod.imagen && !prod.imagen.startsWith('data:') ? prod.imagen : '';
  document.getElementById('prodModalImageFile').value = '';

  const btnSkipImage = document.getElementById('btnSkipImage');
  if (btnSkipImage) btnSkipImage.classList.add('hidden');

  const productModal = document.getElementById('productModal');
  if (productModal) productModal.classList.add('active');
};

window.borrarProductoAdmin = async function(id) {
  const prod = productos.find(p => p.id === id);
  if (!prod) return;

  if (confirm(`¿Estás seguro de eliminar permanentemente "${prod.nombre}"?`)) {
    try {
      await fetch(`${API_URL}/productos/${id}`, { method: 'DELETE' });
      await cargarDatosBackend();
      if (carrito[id]) delete carrito[id];
      renderizarTablaProductosAdmin();
      renderizarProductos(productos);
    } catch (err) {
      console.error("Error al borrar producto:", err);
    }
  }
};

function renderizarTableroPedidos() {
  const ordersBoardContainer = document.getElementById('ordersBoardContainer');
  if (!ordersBoardContainer) return;
  ordersBoardContainer.innerHTML = '';

  if (!usuarioActual || !usuarioActual.sucursal) return;
  const sucursalAdmin = usuarioActual.sucursal.trim().toLowerCase();

  const pedidosPropios = pedidos.filter(p => 
    p.detalles && p.detalles.some(d => d.sucursal && d.sucursal.trim().toLowerCase().replace(/['"`]/g, "") === sucursalAdmin.replace(/['"`]/g, ""))
  );

  if (pedidosPropios.length === 0) {
    ordersBoardContainer.innerHTML = `<p class="no-results">No hay pedidos entrantes para ${usuarioActual.sucursal}.</p>`;
    return;
  }

  pedidosPropios.forEach(p => {
    const detallesSucursal = p.detalles.filter(d => d.sucursal && d.sucursal.trim().toLowerCase().replace(/['"`]/g, "") === sucursalAdmin.replace(/['"`]/g, ""));
    const subtotalSucursal = detallesSucursal.reduce((acc, item) => acc + (item.subtotal || 0), 0);

    const estadoSucursal = detallesSucursal[0]?.estado || "Recibido";
    const esFinalizado = (estadoSucursal === 'Entregado');

    let itemsHtml = detallesSucursal.map(d => `
      <li>${d.cantidad}x ${d.nombre} - <strong>$${(d.subtotal || 0).toFixed(2)}</strong></li>
    `).join('');

    const card = document.createElement('div');
    card.className = `order-card order-status-${estadoSucursal.toLowerCase().replace(/\s+/g, '-')}`;
    card.innerHTML = `
      <div class="order-card-header">
        <h4>Orden #${p.id}</h4>
        <span class="status-tag">${estadoSucursal}</span>
      </div>
      <div class="order-card-body">
        <p><strong>Cliente:</strong> ${p.cliente}</p>
        <p><strong>Teléfono:</strong> ${p.telefono}</p>
        <p><strong>Dirección:</strong> ${p.direccion}</p>
        <p><strong>Pago:</strong> ${p.metodoPago}</p>
        <ul class="order-items-list">${itemsHtml}</ul>
        <p class="order-card-total">Importe Sucursal: $${subtotalSucursal.toFixed(2)}</p>
      </div>
      <div class="order-card-actions">
        <button type="button" class="btn-step ${esFinalizado ? 'disabled' : ''}" 
                id="btn-avanzar-${p.id}" 
                ${esFinalizado ? 'disabled' : ''}>
          ${esFinalizado ? '✔ Pedido Completado' : 'Avanzar Estado ➔'}
        </button>
      </div>
    `;

    ordersBoardContainer.appendChild(card);

    const btnAvanzar = card.querySelector(`#btn-avanzar-${p.id}`);
    if (btnAvanzar && !esFinalizado) {
      btnAvanzar.onclick = () => avanzarEstadoPedidoSucursal(p.id, usuarioActual.sucursal);
    }
  });
}

window.avanzarEstadoPedidoSucursal = async function(idOrden, nombreSucursal) {
  const p = pedidos.find(x => x.id === idOrden);
  if (!p || !p.detalles) return;

  const sucursalTarget = nombreSucursal.trim().toLowerCase().replace(/['"`]/g, "");
  let nuevoEstado = 'Recibido';

  p.detalles.forEach(d => {
    const dSucursalLimpia = d.sucursal ? d.sucursal.trim().toLowerCase().replace(/['"`]/g, "") : "";
    if (dSucursalLimpia === sucursalTarget) {
      if (!d.estado || d.estado === 'Recibido') nuevoEstado = 'En preparación';
      else if (d.estado === 'En preparación') nuevoEstado = 'Enviando';
      else if (d.estado === 'Enviando') nuevoEstado = 'Entregado';
      else nuevoEstado = 'Entregado';
      d.estado = nuevoEstado;
    }
  });

  try {
    await fetch(`${API_URL}/pedidos/${idOrden}/estado`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sucursal: nombreSucursal, estado: nuevoEstado })
    });

    await cargarDatosBackend();
    renderizarTableroPedidos();
  } catch (err) {
    console.error("Error al actualizar estado del pedido:", err);
  }
};