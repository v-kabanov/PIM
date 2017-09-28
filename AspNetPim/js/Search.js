(function ($pim, window, $) {
    $pim.pages.search = {};
    var conf = $pim.config;
    var page = $pim.pages.search;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            $(document).on("change", ":text", trimInputCallback);
            $(document).on("change", "[data-cause-postback]", validateForm);
            //$(document).on("click", conf.deleteNoteButtonSelector, deleteNote);
            pim.features.elementHelper.focusPreserver.init();

            pim.features.autoAjax.init([
                {
                    url: conf.deleteNoteUrl,
                    triggers: [
                        {
                            selector: conf.deleteNoteButtonSelector,
                            eventName: "click"
                        }],
                    confirmFunction: confirmDelete,
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