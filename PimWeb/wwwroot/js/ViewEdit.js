(function ($pim, window, $) {
    $pim.pages.viewEdit = {};
    const conf = $pim.config;
    const page = $pim.pages.viewEdit;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            $(document).on("click", conf.deleteNoteSelector, deleteNote);

            pim.features.autoAjax.init([
                {
                    url: conf.updateNoteUrl,
                    triggers: [
                        {
                            selector: conf.updateNoteButtonSelector,
                            eventName: "click"
                        }],
                    always: setupComponents
                }
            ]);

            setupComponents();
        });

        function setupComponents() {
        }

        function deleteNote(e) {
            if (!confirm("Delete this note?"))
                e.preventDefault();
        }
    };
})(pim, window, jQuery);