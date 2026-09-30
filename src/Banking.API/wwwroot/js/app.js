// NovaBank Digital Banking Frontend Controller

const DEMO_USERS = [
  {
    name: "Ahmet Yılmaz",
    email: "ahmet.yilmaz@banka.com",
    identityNumber: "12345678901"
  },
  {
    name: "Ayşe Demir",
    email: "ayse.demir@banka.com",
    identityNumber: "98765432101"
  }
];

const CURRENCY_SYMBOLS = {
  1: "₺", // TRY
  2: "$", // USD
  3: "€"  // EUR
};

const CURRENCY_NAMES = {
  1: "TRY",
  2: "USD",
  3: "EUR"
};

let state = {
  token: null,
  currentCustomer: null,
  accounts: [],
  selectedAccountId: null,
  allCustomers: []
};

// Initialize Application
document.addEventListener("DOMContentLoaded", async () => {
  setupEventListeners();
  // Auto-login with first demo user for instant interactive experience
  await switchUser(DEMO_USERS[0]);
});

function setupEventListeners() {
  // Tabs
  document.querySelectorAll(".tab-btn").forEach(btn => {
    btn.addEventListener("click", () => {
      document.querySelectorAll(".tab-btn").forEach(b => b.classList.remove("active"));
      btn.classList.add("active");

      const tab = btn.dataset.tab;
      document.getElementById("tab-transfer").style.display = tab === "transfer" ? "block" : "none";
      document.getElementById("tab-deposit").style.display = tab === "deposit" ? "block" : "none";
    });
  });

  // Transfer Form
  const transferForm = document.getElementById("transfer-form");
  if (transferForm) {
    transferForm.addEventListener("submit", handleTransfer);
  }

  // Deposit Form
  const depositForm = document.getElementById("deposit-form");
  if (depositForm) {
    depositForm.addEventListener("submit", handleDeposit);
  }

  // Create Account Form
  const createAccountForm = document.getElementById("create-account-form");
  if (createAccountForm) {
    createAccountForm.addEventListener("submit", handleCreateAccount);
  }

  // Register Customer Form
  const registerForm = document.getElementById("register-customer-form");
  if (registerForm) {
    registerForm.addEventListener("submit", handleRegisterCustomer);
  }

  // Quick Amount Buttons
  document.querySelectorAll(".btn-quick-amount").forEach(btn => {
    btn.addEventListener("click", (e) => {
      const amount = e.target.dataset.amount;
      const targetInput = e.target.closest(".form-group").querySelector("input");
      if (amount === "all") {
        const activeAcc = state.accounts.find(a => a.id === state.selectedAccountId);
        if (activeAcc) targetInput.value = activeAcc.balance;
      } else {
        targetInput.value = amount;
      }
    });
  });
}

// User Authentication & Switcher
async function switchUser(user) {
  try {
    showToast(`"${user.name}" hesabına giriş yapılıyor...`, "info");

    const response = await fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: user.email,
        identityNumber: user.identityNumber
      })
    });

    if (!response.ok) {
      const err = await response.json();
      throw new Error(err.detail || "Giriş yapılamadı.");
    }

    const data = await response.json();
    state.token = data.token;
    state.currentCustomer = {
      id: data.customerId,
      name: data.fullName,
      email: data.email
    };

    updateUserInterface();
    await loadCustomerData();
    showToast(`Hoş geldiniz, ${data.fullName}!`, "success");
  } catch (error) {
    showToast(error.message, "error");
  }
}

function updateUserInterface() {
  document.getElementById("user-name-display").textContent = state.currentCustomer.name;
  document.getElementById("active-user-badge").textContent = state.currentCustomer.name;
  document.getElementById("user-avatar-text").textContent = state.currentCustomer.name
    .split(" ")
    .map(n => n[0])
    .join("");

  document.querySelectorAll(".btn-user-switch").forEach(btn => {
    btn.classList.toggle("active", btn.dataset.email === state.currentCustomer.email);
  });
}

// Data Fetching
async function loadCustomerData() {
  try {
    // 1. Fetch all customers
    const allCustRes = await fetch("/api/customers", {
      headers: { "Authorization": `Bearer ${state.token}` }
    });
    if (allCustRes.ok) {
      state.allCustomers = await allCustRes.json();
    }

    // 2. Fetch and render accounts for active customer
    await populateAccountsUI();
  } catch (error) {
    showToast(error.message, "error");
  }
}

async function populateAccountsUI() {
  const container = document.getElementById("accounts-container");
  const sourceSelect = document.getElementById("transfer-source-account");
  const depositSelect = document.getElementById("deposit-account");

  container.innerHTML = `<div class="empty-state"><span>⏳</span>Hesaplar yükleniyor...</div>`;

  try {
    // Fetch directly from new /api/accounts/customer/{id} endpoint
    const res = await fetch(`/api/accounts/customer/${state.currentCustomer.id}`, {
      headers: { "Authorization": `Bearer ${state.token}` }
    });

    if (!res.ok) throw new Error("Hesaplar yüklenemedi.");

    const accountsData = await res.json();
    state.accounts = accountsData;

    container.innerHTML = "";
    sourceSelect.innerHTML = "";
    depositSelect.innerHTML = "";

    if (accountsData.length === 0) {
      container.innerHTML = `
        <div class="empty-state">
          <span>💳</span>
          <p>Henüz açılmış bir banka hesabınız bulunmuyor.</p>
          <button class="btn-secondary" style="margin-top:1rem;" onclick="openModal('modal-new-account')">➕ Yeni Hesap Aç</button>
        </div>`;
      await loadTransactions(null);
      return;
    }

    if (!state.selectedAccountId || !accountsData.find(a => a.id === state.selectedAccountId)) {
      state.selectedAccountId = accountsData[0].id;
    }

    accountsData.forEach(account => {
      const isSelected = account.id === state.selectedAccountId;
      const card = document.createElement("div");
      card.className = `bank-card ${isSelected ? 'active-card' : ''}`;
      card.onclick = () => selectAccount(account.id);

      const symbol = CURRENCY_SYMBOLS[account.currency] || "₺";
      const currName = CURRENCY_NAMES[account.currency] || "TRY";

      card.innerHTML = `
        <div class="card-top">
          <div class="card-chip"></div>
          <div class="card-currency-tag">${currName}</div>
        </div>
        <div class="card-balance-block">
          <div class="card-balance-label">Kullanılabilir Bakiye</div>
          <div class="card-balance-val">${formatMoney(account.balance)} ${symbol}</div>
        </div>
        <div class="card-bottom">
          <div class="card-iban">${account.iban}</div>
          <button class="card-copy-btn" title="IBAN Kopyala" onclick="event.stopPropagation(); copyToClipboard('${account.iban}')">📋</button>
        </div>
      `;
      container.appendChild(card);

      // Populate Selects
      const opt = document.createElement("option");
      opt.value = account.id;
      opt.textContent = `${currName} - ${account.iban.substring(0, 16)}... (${formatMoney(account.balance)} ${symbol})`;
      if (isSelected) opt.selected = true;
      sourceSelect.appendChild(opt);

      const optDep = opt.cloneNode(true);
      depositSelect.appendChild(optDep);
    });

    // Populate Target Accounts Dropdown
    await populateTransferTargets();

    // Load transactions for the active account
    await loadTransactions(state.selectedAccountId);

  } catch (err) {
    container.innerHTML = `<div class="empty-state"><span>⚠️</span>Hesaplar yüklenirken hata oluştu.</div>`;
  }
}

function selectAccount(accountId) {
  state.selectedAccountId = accountId;
  document.querySelectorAll(".bank-card").forEach(c => c.classList.remove("active-card"));
  const cards = document.querySelectorAll(".bank-card");
  const accIndex = state.accounts.findIndex(a => a.id === accountId);
  if (cards[accIndex]) {
    cards[accIndex].classList.add("active-card");
  }

  const sourceSelect = document.getElementById("transfer-source-account");
  if (sourceSelect) sourceSelect.value = accountId;

  const depositSelect = document.getElementById("deposit-account");
  if (depositSelect) depositSelect.value = accountId;

  loadTransactions(accountId);
}

// Transactions
async function loadTransactions(accountId) {
  const listEl = document.getElementById("transactions-list");
  if (!accountId) {
    listEl.innerHTML = `<div class="empty-state"><span>📜</span>Görüntülenecek hesap hareketi yok.</div>`;
    return;
  }

  listEl.innerHTML = `<div class="empty-state"><span>⏳</span>İşlem hareketleri yükleniyor...</div>`;

  try {
    const res = await fetch(`/api/accounts/${accountId}/transactions`, {
      headers: { "Authorization": `Bearer ${state.token}` }
    });

    if (!res.ok) throw new Error("Hesap hareketleri alınamadı.");
    const transactions = await res.json();

    if (transactions.length === 0) {
      listEl.innerHTML = `<div class="empty-state"><span>🍃</span>Bu hesapta henüz işlem hareketi yok.</div>`;
      return;
    }

    listEl.innerHTML = "";
    transactions.forEach(tx => {
      const isDeposit = tx.transactionType === 1;
      const isWithdrawal = tx.transactionType === 2;
      const isTransfer = tx.transactionType === 3;

      const isIncoming = isDeposit || (isTransfer && tx.targetAccountId === accountId);
      const symbol = CURRENCY_SYMBOLS[tx.currency] || "₺";

      let iconClass = "deposit";
      let icon = "↓";
      if (isTransfer) {
        iconClass = "transfer";
        icon = isIncoming ? "↙" : "↗";
      } else if (isWithdrawal) {
        iconClass = "withdrawal";
        icon = "↑";
      }

      const item = document.createElement("div");
      item.className = "transaction-item";
      item.innerHTML = `
        <div class="tx-left">
          <div class="tx-icon ${iconClass}">${icon}</div>
          <div class="tx-info">
            <h4>${tx.description || 'İşlem'}</h4>
            <span>${new Date(tx.transactionDate).toLocaleString('tr-TR')}</span>
          </div>
        </div>
        <div class="tx-amount ${isIncoming ? 'positive' : 'negative'}">
          ${isIncoming ? '+' : '-'}${formatMoney(tx.amount)} ${symbol}
        </div>
      `;
      listEl.appendChild(item);
    });
  } catch (error) {
    listEl.innerHTML = `<div class="empty-state"><span>⚠️</span>Hareketler yüklenemedi.</div>`;
  }
}

// Transfer Handler
async function handleTransfer(e) {
  e.preventDefault();
  const btn = document.getElementById("btn-submit-transfer");
  btn.disabled = true;
  btn.innerHTML = `<span>⏳</span> İşlem Yürütülüyor...`;

  const sourceAccountId = document.getElementById("transfer-source-account").value;
  const targetAccountId = document.getElementById("transfer-target-account").value;
  const amount = parseFloat(document.getElementById("transfer-amount").value);
  const description = document.getElementById("transfer-description").value;

  try {
    const res = await fetch("/api/transactions/transfer", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${state.token}`
      },
      body: JSON.stringify({
        sourceAccountId,
        targetAccountId,
        amount,
        description: description || "Hesaplar arası transfer"
      })
    });

    const data = await res.json();

    if (!res.ok) {
      const errorMessage = data.detail || (data.errors ? Object.values(data.errors).flat().join(" ") : "Transfer işlemi başarısız.");
      throw new Error(errorMessage);
    }

    showToast(`Transfer başarıyla gerçekleşti! Tutar: ${formatMoney(amount)}`, "success");
    document.getElementById("transfer-amount").value = "";
    document.getElementById("transfer-description").value = "";

    await populateAccountsUI();
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    btn.disabled = false;
    btn.innerHTML = `<span>⚡</span> Güvenli Transfer Yap`;
  }
}

// Deposit Handler
async function handleDeposit(e) {
  e.preventDefault();
  const btn = document.getElementById("btn-submit-deposit");
  btn.disabled = true;
  btn.innerHTML = `<span>⏳</span> Para Yatırılıyor...`;

  const accountId = document.getElementById("deposit-account").value;
  const amount = parseFloat(document.getElementById("deposit-amount").value);
  const description = document.getElementById("deposit-description").value;

  try {
    const res = await fetch(`/api/accounts/${accountId}/deposit`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${state.token}`
      },
      body: JSON.stringify({
        amount,
        description: description || "Hesaba Para Yatırma"
      })
    });

    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.detail || "Para yatırma işlemi başarısız.");
    }

    showToast(`Hesaba ${formatMoney(amount)} yatırıldı! Güncel Bakiye: ${formatMoney(data.balance)}`, "success");
    document.getElementById("deposit-amount").value = "";
    document.getElementById("deposit-description").value = "";

    await populateAccountsUI();
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    btn.disabled = false;
    btn.innerHTML = `<span>💳</span> Hesaba Para Yatır`;
  }
}

// Create Account Handler
async function handleCreateAccount(e) {
  e.preventDefault();
  const currency = parseInt(document.getElementById("new-account-currency").value);
  const initialBalance = parseFloat(document.getElementById("new-account-balance").value || "0");

  try {
    const res = await fetch("/api/accounts", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${state.token}`
      },
      body: JSON.stringify({
        customerId: state.currentCustomer.id,
        currency,
        initialBalance
      })
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.detail || "Hesap açılamadı.");

    showToast(`Yeni ${CURRENCY_NAMES[currency]} hesabı açıldı! IBAN: ${data.iban}`, "success");
    closeModal("modal-new-account");
    await populateAccountsUI();
  } catch (error) {
    showToast(error.message, "error");
  }
}

// Register Customer Handler
async function handleRegisterCustomer(e) {
  e.preventDefault();
  const firstName = document.getElementById("reg-firstname").value;
  const lastName = document.getElementById("reg-lastname").value;
  const email = document.getElementById("reg-email").value;
  const identityNumber = document.getElementById("reg-tckn").value;

  try {
    const res = await fetch("/api/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ firstName, lastName, email, identityNumber })
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.detail || "Müşteri kaydı başarısız.");

    showToast(`Müşteri kaydı tamamlandı! Hoş geldiniz, ${data.fullName}`, "success");
    closeModal("modal-new-customer");

    // Automatically switch to new customer
    await switchUser({ name: data.fullName, email: data.email, identityNumber });
  } catch (error) {
    showToast(error.message, "error");
  }
}

// Helper: Populate Transfer Targets
async function populateTransferTargets() {
  const targetSelect = document.getElementById("transfer-target-account");
  if (!targetSelect) return;

  targetSelect.innerHTML = "";
  let allTargets = [];

  for (const c of state.allCustomers) {
    try {
      const res = await fetch(`/api/accounts/customer/${c.id}`, {
        headers: { "Authorization": `Bearer ${state.token}` }
      });
      if (res.ok) {
        const accs = await res.json();
        accs.forEach(a => allTargets.push({ ...a, ownerName: c.fullName }));
      }
    } catch (e) { }
  }

  // Filter out currently selected source account
  const filtered = allTargets.filter(a => a.id !== state.selectedAccountId);

  if (filtered.length === 0) {
    targetSelect.innerHTML = `<option value="">Transfer edilebilecek başka hesap yok</option>`;
  } else {
    filtered.forEach(acc => {
      const opt = document.createElement("option");
      opt.value = acc.id;
      const symbol = CURRENCY_SYMBOLS[acc.currency] || "₺";
      opt.textContent = `${acc.ownerName} - ${CURRENCY_NAMES[acc.currency]} (${acc.iban.substring(0, 16)}...)`;
      targetSelect.appendChild(opt);
    });
  }
}

// Utilities
function formatMoney(amount) {
  return Number(amount).toLocaleString('tr-TR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
}

function copyToClipboard(text) {
  navigator.clipboard.writeText(text);
  showToast("IBAN panoya kopyalandı! 📋", "info");
}

function showToast(message, type = "info") {
  const container = document.getElementById("toast-container");
  if (!container) return;

  const toast = document.createElement("div");
  toast.className = `toast ${type}`;

  const icon = type === "success" ? "✅" : (type === "error" ? "❌" : "ℹ️");
  toast.innerHTML = `<span>${icon}</span><div>${message}</div>`;

  container.appendChild(toast);

  setTimeout(() => {
    toast.style.opacity = "0";
    toast.style.transform = "translateX(100%)";
    toast.style.transition = "all 0.3s ease";
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

function openModal(id) {
  const modal = document.getElementById(id);
  if (modal) modal.classList.add("open");
}

function closeModal(id) {
  const modal = document.getElementById(id);
  if (modal) modal.classList.remove("open");
}
