// Site geneli JavaScript işlevleri

// Document Ready
$(function () {
    // Bootstrap tooltip'leri etkinleştir
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Bootstrap popovers'ı etkinleştir
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Modal işlemleri
    $('.modal').on('shown.bs.modal', function () {
        $(this).find('[autofocus]').focus();
    });

    // Form validasyonu
    if (typeof ($.fn.validate) !== 'undefined') {
        $.validator.setDefaults({
            highlight: function (element) {
                $(element).addClass('is-invalid');
            },
            unhighlight: function (element) {
                $(element).removeClass('is-invalid');
            },
            errorElement: 'div',
            errorClass: 'invalid-feedback',
            errorPlacement: function (error, element) {
                if (element.parent('.input-group').length) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element);
                }
            }
        });
    }

    // AJAX form gönderimi
    $('form[data-ajax="true"]').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var method = form.attr('method') || 'POST';
        var data = new FormData(form[0]);

        $.ajax({
            url: url,
            method: method,
            data: data,
            processData: false,
            contentType: false,
            beforeSend: function () {
                form.find('button[type="submit"]').prop('disabled', true);
                form.find('.ajax-loading').show();
            },
            success: function (response) {
                if (response.success) {
                    if (response.message) {
                        showNotification('success', response.message);
                    }
                    if (response.redirect) {
                        window.location.href = response.redirect;
                    } else if (form.data('ajax-reset') === true) {
                        form[0].reset();
                    }
                } else {
                    if (response.message) {
                        showNotification('error', response.message);
                    }
                }
            },
            error: function (xhr) {
                var errorMessage = 'İşlem sırasında bir hata oluştu.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                showNotification('error', errorMessage);
            },
            complete: function () {
                form.fin