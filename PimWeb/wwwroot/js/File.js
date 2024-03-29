﻿(function ($pim, window, $) {
    $pim.pages.file = {};
    const conf = $pim.config;
    const page = $pim.pages.file;

    page.init = function (configData) {
        $.extend($pim.config, configData);

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
                always: null,
                getPostData: function (event) {
                    const target = $(event.target || event.srcElement);
                    const data = { noteId: target.attr(conf.attributeNameNoteId) };
                    const postData = $.param(data);
                    return postData;
                }
            }
        ]);

        function deleteFile(e) {
            if (!confirm("Delete this file permanently?"))
                e.preventDefault();
        }

        function confirmDeleteLinkToNote(e) {
            return confirm("Delete association with the note?");
        }
    };
})(pim, window, jQuery);