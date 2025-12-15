// 1. BAĞLANTIYI KUR
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/general-hub")
    .build();

// 2. BAĞLANTIYI BAŞLAT
connection.start().then(function () {
    console.log("✅ SignalR Bağlantısı Başarılı!");
}).catch(function (err) {
    return console.error(err.toString());
});

// ==========================================
// A) YENİ HABER BİLDİRİMİ (Zaten Çalışıyor)
// ==========================================
connection.on("ReceiveNewsNotification", function (title, id) {
    var toastHtml = `
        <div style="position: fixed; bottom: 20px; right: 20px; background: #1cc88a; color: white; padding: 15px; border-radius: 5px; box-shadow: 0 4px 6px rgba(0,0,0,0.2); z-index: 9999; animation: slideIn 0.5s;">
            <i class="fas fa-bullhorn"></i> <strong>YENİ HABER!</strong><br>
            <a href="/News/Details/${id}" style="color: white; text-decoration: underline; font-size:14px;">${title}</a>
        </div>
    `;
    showToast(toastHtml);
});

// ==========================================
// B) YENİ YORUM BİLDİRİMİ (GÜNCELLENDİ 🔥)
// ==========================================
connection.on("ReceiveComment", function (newsId, userName, text, date) {

    // 1. DURUM: Eğer Kullanıcı O Haberi Okuyorsa -> Canlı Listeye Ekle
    var currentUrl = window.location.href;
    if (currentUrl.includes("/News/Details/" + newsId) || currentUrl.includes("id=" + newsId)) {

        var newCommentHtml = `
            <div class="media mb-4 p-3 rounded" style="background-color: #e8f5e9; border-left: 5px solid #1cc88a; animation: fadeIn 1s;">
                <img class="d-flex mr-3 rounded-circle" src="/sbadmin/img/undraw_profile.svg" width="50" height="50" alt="">
                <div class="media-body">
                    <h5 class="mt-0 font-weight-bold text-dark">
                        ${userName} <small class="text-success" style="font-size:0.8rem">(${date})</small>
                    </h5>
                    ${text}
                </div>
            </div>
        `;

        var alertBox = document.querySelector(".alert-info");
        if (alertBox) alertBox.remove();

        var header = document.querySelector(".comments-section h3");
        if (header) {
            header.insertAdjacentHTML('afterend', newCommentHtml);
        }
    }

    // 2. DURUM: Hangi Sayfada Olursa Olsun BİLDİRİM GÖSTER (Admin Paneli Dahil) 🔥
    // Yorumu yapan kişi kendimiz değilsek bildirimi görelim (İsteğe bağlı)

    var commentToastHtml = `
        <div style="position: fixed; bottom: 20px; right: 20px; background: #f6c23e; color: #333; padding: 15px; border-radius: 5px; box-shadow: 0 4px 6px rgba(0,0,0,0.2); z-index: 9999; animation: slideIn 0.5s;">
            <i class="fas fa-comment-dots"></i> <strong>YENİ YORUM!</strong><br>
            <span style="font-size:13px; font-weight:bold;">${userName}:</span>
            <span style="font-size:13px;"> "${text.substring(0, 20)}..."</span><br>
            <a href="/News/Details/${newsId}" style="color: #333; text-decoration: underline; font-size:12px; margin-top:5px; display:block;">Habere Git</a>
        </div>
    `;

    showToast(commentToastHtml);
});

// --- Yardımcı Fonksiyon: Bildirimi Ekrana Bas ve Sil ---
function showToast(htmlContent) {
    document.body.insertAdjacentHTML('beforeend', htmlContent);
    // 5 saniye sonra kaldır
    setTimeout(function () {
        var lastToast = document.body.lastElementChild;
        if (lastToast) lastToast.remove();
    }, 5000);
}

// CSS Animasyonları
var style = document.createElement('style');
style.innerHTML = `
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
`;
document.head.appendChild(style);