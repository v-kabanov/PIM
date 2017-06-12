(function ($pim, window, $) {
    $pim.pages.viewEdit = {};
    var conf = $pim.config;
    var page = $pim.pages.viewEdit;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            $(document).on("change", ":text", trimInputCallback);
            $(document).on("change", "[data-cause-postback]", validateForm);
            $(document).on("click", conf.updateNoteButtonSelector, updateNote);
            $(document).on("click", conf.deleteNoteButtonSelector, deleteNote);
            pim.features.elementHelper.focusPreserver.init();
            setupComponents();
        });

        function setupComponents() {
        }

        function deleteNote(e) {
            e.preventDefault();

            if (confirm("Delete this note ?")) {
                var noteId = $(this).parent().siblings("[name='noteId']").val();

                pim.features.waitingDialog.showPleaseWait();
                ajaxPost(conf.deleteNoteUrl, { noteId: noteId })
                    .always(function () { pim.features.waitingDialog.hidePleaseWait(); });
            }
        }

        // modal
        function updateNote(e) {
            e.preventDefault();
            pim.features.waitingDialog.showPleaseWait();
            ajaxPost(conf.updateNoteUrl)
                .always(function () {
                    pim.features.waitingDialog.hidePleaseWait();
                });;
        }

        // non-modal, async
        function validateForm() {
            if (conf.validationUrl)
                ajaxPost(conf.validationUrl);
        }

        function trimInputCallback() {
            pim.features.elementHelper.trimInput(this);
        }

        function ajaxPost(url, data) {
            $(conf.divInProgressSelector).show();

            if (!data)
                data = $(conf.mainFormSelector).serialize();

            return $.post(url, data, function (result) {
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