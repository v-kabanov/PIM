(function ($pim, window, $) {
    $pim.pages.note = {};
    const conf = $pim.config;
    const page = $pim.pages.note;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).on("click", conf.deleteNoteSelector, deleteNote);

        pim.features.autoAjax.init([
            {
                url: conf.updateNoteUrl,
                triggers: [
                    {
                        selector: conf.updateNoteButtonSelector,
                        eventName: "click"
                    }],
            }, {
                url: conf.deleteAssociationWithFileUrl,
                triggers: [
                    {
                        selector: conf.deleteAssociationWithFileLinkSelector,
                        eventName: "click"
                    }],
                replacementSourceSelector: conf.fileListSelector,
                confirmFunction: confirmDeleteLinkToFile,
                //always: setupComponents,
                getPostData: function (event) {
                    const target = $(event.target || event.srcElement);
                    const data = { fileId: target.attr(conf.attributeNameFileId) };
                    const postData = $.param(data);
                    return postData;
                }
            }
        ]);

        function deleteNote(e) {
            if (!confirm("Delete this note?"))
                e.preventDefault();
        }

        function confirmDeleteLinkToFile(e) {
            return confirm("Delete association with the file?");
        }
    };
})(pim, window, jQuery);