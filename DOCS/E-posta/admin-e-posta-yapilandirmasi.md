# Admin Mail Yapılandırması

Admin panelindeki Mail modülü aynı mailbox için SMTP ve IMAP kullanır. `web.config` içinde yalnızca sunucu ve port gibi secret olmayan değerler tutulmalıdır.

Gerekli environment değerleri:

```text
EmailSettings__SmtpServer=mail.tummenu.com
EmailSettings__Port=465
EmailSettings__Username=destek@tummenu.com
EmailSettings__Password=<secret-store>
EmailSettings__FromEmail=destek@tummenu.com
EmailSettings__FromName=Tüm Menü
EmailSettings__EnableSsl=true
EmailSettings__ImapServer=mail.tummenu.com
EmailSettings__ImapPort=993
EmailSettings__ImapEnableSsl=true
EmailSettings__ConnectionTimeoutSeconds=30
EmailSettings__MaxAttachmentBytes=26214400
```

`EmailSettings__ImapUsername` ve `EmailSettings__ImapPassword` verilmezse IMAP, SMTP kullanıcı adı ve parolasını kullanır. Hosting sağlayıcısı farklı IMAP bilgileri verirse bu iki değer ayrıca tanımlanabilir.

İlk deploy öncesinde mail hesabının parolası döndürülmeli ve gerçek parola IIS environment secret, hosting panelindeki güvenli environment alanı veya benzeri bir secret store üzerinden tanımlanmalıdır. Uygulama 993/SSL IMAP ve 465/SSL SMTP portlarını kullanır; sağlayıcı farklı değer verirse ilgili environment değerleri değiştirilmelidir.
