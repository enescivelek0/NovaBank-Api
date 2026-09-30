// ══════════════════════════════════════════════
//   NovaBank Digital Banking — Frontend Controller
//   Version 2.0
// ══════════════════════════════════════════════

'use strict';

// ─── DEMO USERS ───
const DEMO_USERS = [
  { name: "Ahmet Yılmaz",  email: "ahmet.yilmaz@banka.com",  identityNumber: "12345678901" },
  { name: "Ayşe Demir",    email: "ayse.demir@banka.com",    identityNumber: "98765432101" }
];

// ─── CONSTANTS ───
const CURRENCY_SYMBOL = { 1: "₺", 2: "$", 3: "€" };
const CURRENCY_NAME   = { 1: "TRY", 2: "USD", 3: "EUR" };
const CARD_THEME      = { 1: "theme-try", 2: "theme-usd", 3: "theme-eur" };

// ─── APP STATE ───
let state = {
  token:            null,
  currentCustomer:  null,
  accounts:         [],
  allCustomers:     [],
  selectedAccountId: null,
  transactionFilter: "all"
};

// ═══════════════════════════════════════════
//  INIT
// ═══════════════════════════════════════════
document.addEventListener("DOMContentLoaded", async () => {
  setupNavigation();
  setupTabSwitcher();
  setupFormHandlers();
  setupQuickAmounts();
  setupFilterPills();
  setupSidebar();
  animateMarketTicker();

  // Auto-login first demo user
  await switchUser(DEMO_USERS[0]);
});

// ═══════════════════════════════════════════
//  SIDEBAR & MOBILE NAV
// ═══════════════════════════════════════════
function setupSidebar() {
  const hamburger = document.getElementById("hamburger-btn");
  const sidebar   = document.getElementById("sidebar");
  const overlay   = document.getElementById("sidebar-overlay");

  if (hamburger) {
    hamburger.addEventListener("click", () => {
      sidebar.classList.toggle("mobile-open");
      overlay.classList.toggle("show");
    });
  }
}

function closeSidebar() {
  document.getElementById("sidebar")?.classList.remove("mobile-open");
  document.getElementById("sidebar-overlay")?.classList.remove("show");
}

// ─── Page Navigation ───
function setupNavigation() {
  document.querySelectorAll(".nav-item[data-section]").forEach(link => {
    link.addEventListener("click", e => {
      e.preventDefault();
      const section = link.dataset.section;
      navigateTo(section);
      closeSidebar();
    });
  });
}

function navigateTo(sectionId) {
  // Update active nav
  document.querySelectorAll(".nav-item").forEach(l => l.classList.remove("active"));
  document.querySelector(`.nav-item[data-section="${sectionId}"]`)?.classList.add("active");

  // Show section
  document.querySelectorAll(".page-section").forEach(s => s.classList.remove("active"));
  const target = document.getElementById(`section-${sectionId}`);
  if (target) target.classList.add("active");

  // Update page title
  const titles = {
    dashboard:    "Genel Bakış",
    accounts:     "Hesaplarım",
    transfer:     "Para Transferi",
    transactions: "İşlem Geçmişi"
  };
  const titleEl = document.getElementById("page-title");
  if (titleEl) titleEl.textContent = titles[sectionId] || "NovaBank";
}

// ═══════════════════════════════════════════
//  MARKET TICKER SIMULATION
// ═══════════════════════════════════════════
function animateMarketTicker() {
  const usdBase = 34.22;
  const eurBase = 38.15;

  setInterval(() => {
    const usdEl = document.getElementById("rate-usd");
    const eurEl = document.getElementById("rate-eur");
    if (usdEl) {
      const drift = (Math.random() - 0.5) * 0.05;
      usdEl.textContent = (usdBase + drift).toFixed(2);
    }
    if (eurEl) {
      const drift = (Math.random() - 0.5) * 0.05;
      eurEl.textContent = (eurBase + drift).toFixed(2);
    }
  }, 3500);
}

// ═══════════════════════════════════════════
//  TAB SWITCHER
// ═══════════════════════════════════════════
function setupTabSwitcher() {
  document.querySelectorAll(".tab-btn").forEach(btn => {
    btn.addEventListener("click", () => {
      const tab = btn.dataset.tab;

      // Update buttons
      document.querySelectorAll(".tab-btn").forEach(b => b.classList.remove("active"));
      btn.classList.add("active");

      // Update panels
      document.querySelectorAll(".tab-content").forEach(p => p.classList.remove("active"));
      const panel = document.getElementById(`tab-${tab}`);
      if (panel) panel.classList.add("active");
    });
  });
}

// ═══════════════════════════════════════════
//  QUICK AMOUNT BUTTONS
// ═══════════════════════════════════════════
function setupQuickAmounts() {
  document.querySelectorAll(".quick-amt-btn").forEach(btn => {
    btn.addEventListener("click", () => {
      const amount  = btn.dataset.amount;
      const form    = btn.closest("form");
      if (!form) return;
      const input   = form.querySelector("input[type='number']");
      if (!input) return;

      if (amount === "all") {
        const acc = state.accounts.find(a => a.id === state.selectedAccountId);
        if (acc) input.value = acc.balance;
      } else {
        input.value = amount;
      }
    });
  });
}

// ═══════════════════════════════════════════
//  FILTER PILLS
// ═══════════════════════════════════════════
function setupFilterPills() {
  document.querySelectorAll(".filter-pill").forEach(btn => {
    btn.addEventListener("click", () => {
      document.querySelectorAll(".filter-pill").forEach(b => b.classList.remove("active"));
      btn.classList.add("active");
      state.transactionFilter = btn.dataset.filter;
      renderCurrentTransactions();
    });
  });
}

// Store raw transactions for filter
let _lastTransactions = [];
function renderCurrentTransactions() {
  renderTransactions(_lastTransactions, state.selectedAccountId);
}

// ═══════════════════════════════════════════
//  FORM HANDLERS
// ═══════════════════════════════════════════
function setupFormHandlers() {
  document.getElementById("transfer-form")?.addEventListener("submit", handleTransfer);
  document.getElementById("deposit-form")?.addEventListener("submit", handleDeposit);
  document.getElementById("create-account-form")?.addEventListener("submit", handleCreateAccount);
  document.getElementById("register-customer-form")?.addEventListener("submit", handleRegisterCustomer);

  // Source account balance hint
  document.getElementById("transfer-source-account")?.addEventListener("change", updateBalanceHint);
}

function updateBalanceHint() {
  const sourceId  = document.getElementById("transfer-source-account")?.value;
  const hintEl    = document.getElementById("source-acc-balance-hint");
  if (!hintEl) return;
  const acc = state.accounts.find(a => a.id === sourceId);
  if (acc) {
    const sym = CURRENCY_SYMBOL[acc.currency] || "₺";
    hintEl.textContent = `Bakiye: ${formatMoney(acc.balance)} ${sym}`;
  } else {
    hintEl.textContent = "Bakiye: —";
  }
}

// ═══════════════════════════════════════════
//  AUTH — USER SWITCHING
// ═══════════════════════════════════════════
async function switchUser(user) {
  try {
    showToast(`"${user.name}" hesabına giriş yapılıyor...`, "info");

    const res = await fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email: user.email, identityNumber: user.identityNumber })
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.detail || "Giriş yapılamadı.");
    }

    const data      = await res.json();
    state.token     = data.token;
    state.currentCustomer = {
      id:    data.customerId,
      name:  data.fullName,
      email: data.email
    };
    state.selectedAccountId = null;

    updateUIForUser();
    updateDemoButtons(user.email);
    await loadCustomerData();
    showToast(`Hoş geldiniz, ${data.fullName}! 🎉`, "success");
  } catch (err) {
    showToast(err.message, "error");
  }
}

function updateUIForUser() {
  const name = state.currentCustomer.name;
  const initials = name.split(" ").map(n => n[0]).join("").substring(0, 2).toUpperCase();

  ["sidebar-name", "topbar-name"].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.textContent = name;
  });

  ["sidebar-avatar", "topbar-avatar"].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.textContent = initials;
  });
}

function updateDemoButtons(email) {
  DEMO_USERS.forEach((u, i) => {
    const btn = document.getElementById(`demo-btn-${i}`);
    if (btn) btn.classList.toggle("active", u.email === email);
  });
}

// ═══════════════════════════════════════════
//  DATA LOADING
// ═══════════════════════════════════════════
async function loadCustomerData() {
  try {
    // Load all customers for transfer target dropdown
    const allRes = await fetch("/api/customers", {
      headers: { "Authorization": `Bearer ${state.token}` }
    });
    if (allRes.ok) {
      state.allCustomers = await allRes.json();
    }

    await refreshAccountsUI();
  } catch (err) {
    showToast(err.message, "error");
  }
}

async function refreshAccountsUI() {
  const container = document.getElementById("accounts-container");

  if (container) {
    container.innerHTML = `
      <div class="loading-placeholder">
        <div class="spinner"></div>
        <span>Hesaplar yükleniyor...</span>
      </div>`;
  }

  try {
    const res = await fetch(`/api/accounts/customer/${state.currentCustomer.id}`, {
      headers: { "Authorization": `Bearer ${state.token}` }
    });

    if (!res.ok) throw new Error("Hesaplar yüklenemedi.");

    state.accounts = await res.json();

    // Update badge
    const badge = document.getElementById("accounts-count-badge");
    if (badge) badge.textContent = state.accounts.length;

    // Set selected
    if (!state.selectedAccountId || !state.accounts.find(a => a.id === state.selectedAccountId)) {
      state.selectedAccountId = state.accounts[0]?.id || null;
    }

    renderAccountCards();
    populateAccountSelects();
    await populateTransferTargets();
    await loadTransactions(state.selectedAccountId);

    // KPI totals
    updateNetWorth();

  } catch (err) {
    if (container) {
      container.innerHTML = `
        <div class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="12" cy="12" r="10"/><path d="M12 8v4M12 16h.01"/>
          </svg>
          <p>Hesaplar yüklenirken bir hata oluştu.</p>
        </div>`;
    }
  }
}

// ─── Render Account Cards ───
function renderAccountCards() {
  const container = document.getElementById("accounts-container");
  if (!container) return;

  if (state.accounts.length === 0) {
    container.innerHTML = `
      <div class="empty-state" style="grid-column: 1 / -1;">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <rect x="2" y="5" width="20" height="14" rx="2"/><path d="M2 10h20"/>
        </svg>
        <p>Henüz açılmış banka hesabınız bulunmuyor.</p>
        <button class="btn-outline-primary" onclick="openModal('modal-new-account')" style="margin-top: 0.75rem;">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 5v14M5 12h14"/></svg>
          Yeni Hesap Aç
        </button>
      </div>`;
    return;
  }

  container.innerHTML = "";

  state.accounts.forEach(acc => {
    const isActive = acc.id === state.selectedAccountId;
    const sym      = CURRENCY_SYMBOL[acc.currency] || "₺";
    const cur      = CURRENCY_NAME[acc.currency]   || "TRY";
    const theme    = CARD_THEME[acc.currency]      || "theme-try";

    const card = document.createElement("div");
    card.className = `bank-card ${theme} ${isActive ? "active-card" : ""}`;
    card.setAttribute("role", "button");
    card.setAttribute("aria-label", `${cur} hesabı — ${formatMoney(acc.balance)} ${sym}`);
    card.onclick = () => selectAccount(acc.id);

    // Mask IBAN for display
    const ibanDisplay = acc.iban ? acc.iban.replace(/(.{4})/g, "$1 ").trim() : "—";

    card.innerHTML = `
      <div class="card-top">
        <div class="card-chip"></div>
        <div class="card-badge">
          <span class="card-contactless">◉</span>
          <span class="card-type-pill">${cur}</span>
        </div>
      </div>
      <div class="card-balance-section">
        <div class="card-balance-label">Kullanılabilir Bakiye</div>
        <div class="card-balance-amount">${formatMoney(acc.balance)} <small style="font-size:1.1rem;opacity:0.8">${sym}</small></div>
      </div>
      <div class="card-bottom">
        <div class="card-iban">${ibanDisplay}</div>
        <button class="card-copy-btn" title="IBAN Kopyala" onclick="event.stopPropagation(); copyIban('${acc.iban}')">
          📋 Kopyala
        </button>
      </div>`;

    container.appendChild(card);
  });
}

// ─── Populate Selects ───
function populateAccountSelects() {
  const sourceSelect  = document.getElementById("transfer-source-account");
  const depositSelect = document.getElementById("deposit-account");

  [sourceSelect, depositSelect].forEach(sel => {
    if (!sel) return;
    sel.innerHTML = "";
    state.accounts.forEach(acc => {
      const sym = CURRENCY_SYMBOL[acc.currency] || "₺";
      const cur = CURRENCY_NAME[acc.currency]   || "TRY";
      const opt = document.createElement("option");
      opt.value = acc.id;
      opt.textContent = `${cur} · ${acc.iban?.substring(0, 18)}... · ${formatMoney(acc.balance)} ${sym}`;
      if (acc.id === state.selectedAccountId) opt.selected = true;
      sel.appendChild(opt);
    });
  });

  updateBalanceHint();
}

// ─── Populate Transfer Targets ───
async function populateTransferTargets() {
  const targetSelect = document.getElementById("transfer-target-account");
  if (!targetSelect) return;

  targetSelect.innerHTML = `<option value="">Yükleniyor...</option>`;
  const allTargets = [];

  for (const c of state.allCustomers) {
    try {
      const res = await fetch(`/api/accounts/customer/${c.id}`, {
        headers: { "Authorization": `Bearer ${state.token}` }
      });
      if (res.ok) {
        const accs = await res.json();
        accs.forEach(a => allTargets.push({ ...a, ownerName: c.fullName || `${c.firstName} ${c.lastName}` }));
      }
    } catch (_) { /* skip */ }
  }

  const filtered = allTargets.filter(a => a.id !== state.selectedAccountId);
  targetSelect.innerHTML = "";

  if (filtered.length === 0) {
    targetSelect.innerHTML = `<option value="">Transfer edilebilecek başka hesap yok</option>`;
  } else {
    filtered.forEach(acc => {
      const sym = CURRENCY_SYMBOL[acc.currency] || "₺";
      const cur = CURRENCY_NAME[acc.currency]   || "TRY";
      const opt = document.createElement("option");
      opt.value = acc.id;
      opt.textContent = `${acc.ownerName} · ${cur} · ${acc.iban?.substring(0, 18)}...`;
      targetSelect.appendChild(opt);
    });
  }
}

// ─── Select Account ───
function selectAccount(accountId) {
  state.selectedAccountId = accountId;

  // Re-render cards
  document.querySelectorAll(".bank-card").forEach((card, i) => {
    const acc = state.accounts[i];
    card.classList.toggle("active-card", acc?.id === accountId);
  });

  // Sync selects
  const sourceSelect = document.getElementById("transfer-source-account");
  if (sourceSelect) sourceSelect.value = accountId;
  const depositSelect = document.getElementById("deposit-account");
  if (depositSelect) depositSelect.value = accountId;

  updateBalanceHint();
  loadTransactions(accountId);
}

// ─── Net Worth KPI ───
function updateNetWorth() {
  const total  = state.accounts.reduce((sum, a) => sum + (a.balance || 0), 0);
  const el     = document.getElementById("networth-display");
  if (el) el.textContent = `${formatMoney(total)} ₺`;
}

// ═══════════════════════════════════════════
//  TRANSACTIONS
// ═══════════════════════════════════════════
async function loadTransactions(accountId) {
  const feedEl = document.getElementById("transactions-list");
  if (!feedEl) return;

  if (!accountId) {
    feedEl.innerHTML = `
      <div class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M9 12h6M9 16h4M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
        </svg>
        <p>Görüntülenecek hesap yok.</p>
      </div>`;
    return;
  }

  feedEl.innerHTML = `
    <div class="loading-placeholder">
      <div class="spinner"></div>
      <span>İşlemler yükleniyor...</span>
    </div>`;

  try {
    const res = await fetch(`/api/accounts/${accountId}/transactions`, {
      headers: { "Authorization": `Bearer ${state.token}` }
    });

    if (!res.ok) throw new Error("İşlem hareketleri alınamadı.");

    _lastTransactions = await res.json();
    renderTransactions(_lastTransactions, accountId);
  } catch (err) {
    feedEl.innerHTML = `
      <div class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <circle cx="12" cy="12" r="10"/><path d="M12 8v4M12 16h.01"/>
        </svg>
        <p>Hareketler yüklenemedi.</p>
      </div>`;
  }
}

function renderTransactions(transactions, accountId) {
  const feedEl = document.getElementById("transactions-list");
  if (!feedEl) return;

  const filter = state.transactionFilter;

  const filtered = transactions.filter(tx => {
    if (filter === "all")      return true;
    if (filter === "transfer") return tx.transactionType === 3;
    if (filter === "deposit")  return tx.transactionType === 1;
    return true;
  });

  if (filtered.length === 0) {
    feedEl.innerHTML = `
      <div class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path d="M9 12h6M9 16h4M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
        </svg>
        <p>Bu kategoride işlem hareketi bulunmuyor.</p>
      </div>`;
    return;
  }

  feedEl.innerHTML = "";

  filtered.forEach(tx => {
    const isDeposit    = tx.transactionType === 1;
    const isWithdrawal = tx.transactionType === 2;
    const isTransfer   = tx.transactionType === 3;

    const isIncoming = isDeposit || (isTransfer && tx.targetAccountId === accountId);
    const sym        = CURRENCY_SYMBOL[tx.currency] || "₺";

    let iconClass = "deposit";
    let iconHtml  = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/>
    </svg>`;

    if (isTransfer) {
      iconClass = "transfer";
      iconHtml = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M7 16V4m0 0L3 8m4-4l4 4M17 8v12m0 0l4-4m-4 4l-4-4"/>
      </svg>`;
    } else if (isWithdrawal) {
      iconClass = "withdrawal";
      iconHtml = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <polyline points="23 18 13.5 8.5 8.5 13.5 1 6"/><polyline points="17 18 23 18 23 12"/>
      </svg>`;
    }

    const amountClass = isIncoming ? "credit" : "debit";
    const amountSign  = isIncoming ? "+" : "−";

    const item = document.createElement("div");
    item.className = "tx-item";
    item.innerHTML = `
      <div class="tx-icon ${iconClass}">${iconHtml}</div>
      <div class="tx-info">
        <div class="tx-desc">${tx.description || "İşlem"}</div>
        <div class="tx-date">${new Date(tx.transactionDate).toLocaleString("tr-TR")}</div>
      </div>
      <div class="tx-amount ${amountClass}">${amountSign}${formatMoney(tx.amount)} ${sym}</div>`;

    feedEl.appendChild(item);
  });
}

// ═══════════════════════════════════════════
//  TRANSFER HANDLER
// ═══════════════════════════════════════════
async function handleTransfer(e) {
  e.preventDefault();
  const btn = document.getElementById("btn-submit-transfer");
  setButtonLoading(btn, true, "İşlem Yürütülüyor...");

  const sourceAccountId = document.getElementById("transfer-source-account").value;
  const targetAccountId = document.getElementById("transfer-target-account").value;
  const amount          = parseFloat(document.getElementById("transfer-amount").value);
  const description     = document.getElementById("transfer-description").value;

  try {
    const res = await fetch("/api/transactions/transfer", {
      method: "POST",
      headers: {
        "Content-Type":  "application/json",
        "Authorization": `Bearer ${state.token}`
      },
      body: JSON.stringify({
        sourceAccountId,
        targetAccountId,
        amount,
        description: description || "Hesaplar arası FAST transferi"
      })
    });

    const data = await res.json();
    if (!res.ok) {
      const msg = data.detail || (data.errors ? Object.values(data.errors).flat().join(" ") : "Transfer başarısız.");
      throw new Error(msg);
    }

    // Show receipt
    showReceipt({
      type:         "FAST Transferi",
      sender:       state.currentCustomer.name,
      senderIban:   state.accounts.find(a => a.id === sourceAccountId)?.iban || "—",
      receiver:     document.getElementById("transfer-target-account").selectedOptions[0]?.text || "Alıcı",
      amount:       `${formatMoney(amount)} ₺`,
      description:  description || "Hesaplar arası FAST transferi"
    });

    document.getElementById("transfer-amount").value      = "";
    document.getElementById("transfer-description").value = "";

    showToast(`✅ Transfer başarılı! ${formatMoney(amount)} ₺ gönderildi.`, "success");
    await refreshAccountsUI();
  } catch (err) {
    showToast(err.message, "error");
  } finally {
    setButtonLoading(btn, false, `
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width:18px;height:18px">
        <path d="M13 10V3L4 14h7v7l9-11h-7z"/>
      </svg>
      Güvenli FAST Transferi Başlat`);
  }
}

// ═══════════════════════════════════════════
//  DEPOSIT HANDLER
// ═══════════════════════════════════════════
async function handleDeposit(e) {
  e.preventDefault();
  const btn = document.getElementById("btn-submit-deposit");
  setButtonLoading(btn, true, "Para Yatırılıyor...");

  const accountId   = document.getElementById("deposit-account").value;
  const amount      = parseFloat(document.getElementById("deposit-amount").value);
  const description = document.getElementById("deposit-description").value;

  try {
    const res = await fetch(`/api/accounts/${accountId}/deposit`, {
      method: "POST",
      headers: {
        "Content-Type":  "application/json",
        "Authorization": `Bearer ${state.token}`
      },
      body: JSON.stringify({ amount, description: description || "Hesaba Para Yatırma" })
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.detail || "Para yatırma başarısız.");

    document.getElementById("deposit-amount").value      = "";
    document.getElementById("deposit-description").value = "";

    showToast(`💳 ${formatMoney(amount)} ₺ hesaba yatırıldı! Bakiye: ${formatMoney(data.balance)} ₺`, "success");
    await refreshAccountsUI();
  } catch (err) {
    showToast(err.message, "error");
  } finally {
    setButtonLoading(btn, false, `
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width:18px;height:18px">
        <path d="M12 5v14M5 12h14"/>
      </svg>
      Hesaba Anında Para Yatır`);
  }
}

// ═══════════════════════════════════════════
//  CREATE ACCOUNT HANDLER
// ═══════════════════════════════════════════
async function handleCreateAccount(e) {
  e.preventDefault();
  const currency       = parseInt(document.getElementById("new-account-currency").value);
  const initialBalance = parseFloat(document.getElementById("new-account-balance").value || "0");

  try {
    const res = await fetch("/api/accounts", {
      method: "POST",
      headers: {
        "Content-Type":  "application/json",
        "Authorization": `Bearer ${state.token}`
      },
      body: JSON.stringify({ customerId: state.currentCustomer.id, currency, initialBalance })
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.detail || "Hesap açılamadı.");

    showToast(`🏦 Yeni ${CURRENCY_NAME[currency]} hesabı açıldı! IBAN: ${data.iban}`, "success");
    closeModal("modal-new-account");
    await refreshAccountsUI();
  } catch (err) {
    showToast(err.message, "error");
  }
}

// ═══════════════════════════════════════════
//  REGISTER CUSTOMER HANDLER
// ═══════════════════════════════════════════
async function handleRegisterCustomer(e) {
  e.preventDefault();
  const firstName      = document.getElementById("reg-firstname").value;
  const lastName       = document.getElementById("reg-lastname").value;
  const email          = document.getElementById("reg-email").value;
  const identityNumber = document.getElementById("reg-tckn").value;

  try {
    const res = await fetch("/api/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ firstName, lastName, email, identityNumber })
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.detail || "Müşteri kaydı başarısız.");

    showToast(`🎉 Müşteri kaydı tamamlandı! Hoş geldiniz, ${data.fullName}`, "success");
    closeModal("modal-new-customer");

    // Auto login as new customer
    await switchUser({ name: data.fullName, email: data.email, identityNumber });
  } catch (err) {
    showToast(err.message, "error");
  }
}

// ═══════════════════════════════════════════
//  QUICK FILL PAYEE
// ═══════════════════════════════════════════
function quickFillPayee(name, iban, description) {
  const targetSelect = document.getElementById("transfer-target-account");
  const descInput    = document.getElementById("transfer-description");

  if (descInput) descInput.value = description;

  // Try to find matching IBAN option
  if (targetSelect) {
    const opts = Array.from(targetSelect.options);
    const match = opts.find(o => o.text.includes(iban.substring(0, 10)));
    if (match) targetSelect.value = match.value;
  }

  // Switch to transfer tab
  const transferTabBtn = document.getElementById("tab-btn-transfer");
  if (transferTabBtn && !transferTabBtn.classList.contains("active")) {
    transferTabBtn.click();
  }

  showToast(`${name} seçildi — tutarı girerek transferi tamamlayın.`, "info");
}

// ═══════════════════════════════════════════
//  RECEIPT
// ═══════════════════════════════════════════
function showReceipt({ type, sender, senderIban, receiver, amount, description }) {
  const now = new Date();
  const dateStr = now.toLocaleString("tr-TR", {
    day: "2-digit", month: "2-digit", year: "numeric",
    hour: "2-digit", minute: "2-digit"
  });

  const refNo = "NVB-" + Math.floor(Math.random() * 900000000 + 100000000);
  const uuid  = crypto.randomUUID ? crypto.randomUUID().split("-")[0].toUpperCase() : "XXXXXX";

  setEl("receipt-date-field", dateStr);
  setEl("receipt-ref-no", refNo);
  setEl("receipt-type", type);
  setEl("receipt-sender", sender);
  setEl("receipt-sender-iban", senderIban?.replace(/(.{4})/g, "$1 ").trim());
  setEl("receipt-receiver", receiver);
  setEl("receipt-amount", amount);
  setEl("receipt-desc", description);
  setEl("receipt-uuid", uuid);

  openModal("modal-receipt");
}

function printReceipt() {
  window.print();
}

// ═══════════════════════════════════════════
//  MODAL
// ═══════════════════════════════════════════
function openModal(id) {
  const el = document.getElementById(id);
  if (el) {
    el.classList.add("open");
    document.body.style.overflow = "hidden";
  }
}

function closeModal(id) {
  const el = document.getElementById(id);
  if (el) {
    el.classList.remove("open");
    document.body.style.overflow = "";
  }
}

// Close modal on backdrop click
document.addEventListener("click", e => {
  if (e.target.classList.contains("modal-overlay")) {
    e.target.classList.remove("open");
    document.body.style.overflow = "";
  }
});

// Close modal on ESC
document.addEventListener("keydown", e => {
  if (e.key === "Escape") {
    document.querySelectorAll(".modal-overlay.open").forEach(m => {
      m.classList.remove("open");
      document.body.style.overflow = "";
    });
  }
});

// ═══════════════════════════════════════════
//  TOAST NOTIFICATIONS
// ═══════════════════════════════════════════
function showToast(message, type = "info") {
  const shelf = document.getElementById("toast-shelf");
  if (!shelf) return;

  const icons = {
    success: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg>`,
    error:   `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M18 6L6 18M6 6l12 12"/></svg>`,
    info:    `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4M12 8h.01"/></svg>`
  };

  const toast = document.createElement("div");
  toast.className = `toast ${type}`;
  toast.innerHTML = `
    <div class="toast-icon">${icons[type] || icons.info}</div>
    <div class="toast-body">${message}</div>`;

  shelf.appendChild(toast);

  setTimeout(() => {
    toast.style.transition = "all 0.35s ease";
    toast.style.opacity    = "0";
    toast.style.transform  = "translateX(50px)";
    setTimeout(() => toast.remove(), 350);
  }, 4500);
}

// ═══════════════════════════════════════════
//  UTILITIES
// ═══════════════════════════════════════════
function formatMoney(amount) {
  return Number(amount || 0).toLocaleString("tr-TR", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
}

function copyIban(iban) {
  if (!iban) return;
  navigator.clipboard?.writeText(iban).then(() => {
    showToast(`📋 IBAN panoya kopyalandı: ${iban}`, "info");
  }).catch(() => {
    showToast("IBAN kopyalanamadı.", "error");
  });
}

function setEl(id, val) {
  const el = document.getElementById(id);
  if (el) el.textContent = val;
}

function setButtonLoading(btn, loading, content) {
  if (!btn) return;
  btn.disabled     = loading;
  btn.innerHTML    = loading
    ? `<div class="spinner" style="width:20px;height:20px;border-width:2px;"></div> ${content}`
    : content;
}
