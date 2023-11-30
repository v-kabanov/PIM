(function ($pim, window, $) {
    $pim.pages.file = {};
    const conf = $pim.config;
    const page = $pim.pages.file;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            $(document).on("click", conf.deleteButtonSelector, deleteFile);

            pim.features.autoAjax.init([
                {
                    url: conf.updateUrl,
                    triggers: [
                        {
                            selector: conf.updateButtonSelector,
                            eventName: "click"
                        }],
                    always: setupComponents
                }, {
                    url: conf.deleteAssociationWithNoteUrl,
                    triggers: [
                        {
                            selector: conf.deleteAssociationWithNoteLinkSelector,
                            eventName: "click"
                        }],
                    replacementSourceSelector: conf.noteListSelector,
                    confirmFunction: confirmDeleteLinkToNote,
                    always: setupComponents,
                    getPostData: function (event) {
                        const target = $(event.target || event.srcElement);
                        const data = { noteId: target.attr(conf.attributeNameNoteId) };
                        const postData = $.param(data);
                        return postData;
                    }
                }
            ]);

            setupComponents();
        });

        function setupComponents() {
        }

        function deleteFile(e) {
            if (!confirm("Delete this file permanently?"))
                e.preventDefault();
        }

        function confirmDeleteLinkToNote(e) {
            if (!confirm("Delete association with the note?"))
                e.preventDefault();
        }
    };
})(pim, window, jQuery);