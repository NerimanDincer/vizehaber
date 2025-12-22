// --- 1. SIGNALR BAĞLANTISI ---
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/general-hub")
    .build();

connection.start().then(function () {
    console.log("✅ SignalR Bağlantısı Başarılı!");
}).catch(function (err) {
    return console.error(err.toString());
});

// --- 2. YENİ YORUM GELİNCE ÇALIŞACAK KISIM ---
connection.on("ReceiveComment", function (newsId, userName, text, date) {

    // Eğer kullanıcı şu an o haberi okuyorsa, yorumu listeye ekle
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
            </div>`;

        // "Henüz yorum yok" yazısını kaldır
        var alertBox = document.querySelector(".alert-info");
        if (alertBox) alertBox.remove();

        // Yorum başlığının altına yeni yorumu ekle
        var header = document.querySelector(".comments-section h3");
        if (header) {
            header.insertAdjacentHTML('afterend', newCommentHtml);
        }
    }

    // Sağ altta bildirim çıkar
    showToast(userName, text);
});

// --- 3. YARDIMCI: BİLDİRİM KUTUSU (Toast) ---
function showToast(userName, text) {
    var toastHtml = `
        <div style="position: fixed; bottom: 20px; right: 20px; background: #f6c23e; color: #333; padding: 15px; border-radius: 5px; box-shadow: 0 4px 6px rgba(0,0,0,0.2); z-index: 9999;">
            <strong>💬 ${userName}:</strong> ${text.substring(0, 20)}...
        </div>`;

    document.body.insertAdjacentHTML('beforeend', toastHtml);
    setTimeout(function () {
        var lastToast = document.body.lastElementChild;
        if (lastToast) lastToast.remove();
    }, 4000);
}

// --- 4. VE ASIL KURTARICI: AJAX İLE YORUM GÖNDERME ---
$(document).ready(function () {

    // Formun gönderilmesini yakalıyoruz
    $("#commentForm").submit(function (e) {

        // DUR YOLCU! Sayfayı yenileme.
        e.preventDefault();

        var formData = $(this).serialize(); // Verileri al
        var formAction = $(this).attr("action"); // Nereye gidecek? (/News/AddComment)

        $.ajax({
            url: formAction, // Veya direkt '/News/AddComment' yazabilirsin
            type: 'POST',
            data: formData,
            success: function (response) {
                // Başarılı olursa kutuyu temizle
                $("textarea[name='text']").val("");
                console.log("Yorum arka planda gönderildi!");
            },
            error: function (xhr) {
                console.error("Hata:", xhr.responseText);
                alert("Yorum gönderilirken hata oluştu.");
            }
        });
    });
});
