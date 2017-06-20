(function ($pim, window, $) {
    $pim.pages.latestAndCreate = {};
    var conf = $pim.config;
    var page = $pim.pages.latestAndCreate;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            $(document).on("change", ":text", trimInputCallback);
            pim.features.elementHelper.focusPreserver.init();

            pim.features.autoAjax.init([
                {
                    url: conf.deleteNoteUrl,
                    selector: conf.deleteNoteButtonSelector,
                    event: "click",
                    confirmFunction: confirmDelete,
                    always: setupComponents
                }
                , {
                    url: conf.createNoteUrl,
                    selector: conf.createNoteButtonSelector,
                    event: "click",
                    success: function() { $(conf.newNoteTextSelector).val(""); },
                    always: setupComponents
                }
            ]);

            setupComponents();
        });

        function setupComponents() {
        }

        function confirmDelete(event) {
            var noteName = $(event.target).parent().siblings("div[note-name]").find("a[note-name]").text();
            return confirm("Delete " + $.trim(noteName) + "?");
        }

        function trimInputCallback() {
            pim.features.elementHelper.trimInput(this);
        }
    };
})(pim, window, jQuery);