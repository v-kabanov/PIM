(function ($pim, window, $) {
    $pim.pages.search = {};
    var conf = $pim.config;
    var page = $pim.pages.search;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            $(document).on("change", ":text", trimInputCallback);
            $(document).on("change", "[data-cause-postback]", validateForm);
            $(document).on("click", conf.deleteNoteButtonSelector, deleteNote);
            pim.features.elementHelper.focusPreserver.init();
            setupComponents();
        });

        function setupComponents() {
        }

        function deleteNote(e) {
            e.preventDefault();

            var noteName = $(this).parent().siblings("div[note-name]").find("a[note-name]").text();
            if (confirm("Delete " + $.trim(noteName) + "?")) {
                var noteId = $(this).parent().siblings("[name='noteId']").val();

                pim.features.waitingDialog.showPleaseWait();
                ajaxPost(conf.deleteNoteUrl)
                    .always(function () { pim.features.waitingDialog.hidePleaseWait(); });
            }
        }

        // non-modal, async
        function validateForm() {
            if (conf.validationUrl)
                ajaxPost(conf.validationUrl);
        }

        function trimInputCallback() {
            pim.features.elementHelper.trimInput(this);
        }

        function ajaxPost(url, additionalData) {
            $(conf.divInProgressSelector).show();

            var formData = $(conf.mainFormSelector).serialize();
            if (additionalData)
                $.extend(formData, additionalData);

            return $.post(url, formData, function (result) {
                var $result = $(result);
                ///having <script> in html result interferes with jquery validation. 
                /// need to run manually (not using browser default behavior)
                var scripts = $result.find("script").add($result.filter("script"));
                scripts.each(function (ind, val) {
                    eval($(val).html());
                });
                var form = $result.find("form").add($result.filter("form"));
                $(conf.mainFormSelector).html(form.html());
            }).fail(function (data) {
                var formMessageElem = $(conf.divFormMessagesSelector);
                formMessageElem.append($("<p>").addClass("field-validation-error").text("Error: " + data));
            }).always(function (data) {
                pim.features.elementHelper.loadTooltipWithValidation();
                setupComponents();
                $(conf.divInProgressSelector).hide();
                pim.features.elementHelper.focusPreserver.preserve();
            });
        }
    };
})(pim, window, jQuery);