(function ($pim, window, $) {
    $pim.pages.latestAndCreate = {};
    const conf = $pim.config;
    const page = $pim.pages.latestAndCreate;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        pim.features.autoAjax.init([
            {
                url: conf.deleteNoteUrl,
                triggers: [
                    {
                        selector: conf.deleteNoteSelector,
                        eventName: "click"
                    }],
                replacementSourceSelector: conf.divNoteListSelector,
                replacementTargetSelector: conf.divNoteListSelector,
                confirmFunction: confirmDelete,
                always: null,
                getPostData: function(event) {
                    const target = $(event.target || event.srcElement);
                    const data = [{
                        name: conf.attributeNameNoteId,
                        value: target.attr(conf.attributeNameNoteId)
                    }];
                    const postData = $.param(data);
                    return postData;
                },
            }
            , {
                url: conf.createNoteUrl,
                triggers: [
                    {
                        selector: conf.createNoteButtonSelector,
                        eventName: "click"
                    }],
                replacementSourceSelector: conf.divNoteListSelector,
                replacementTargetSelector: conf.divNoteListSelector,
                success: function () { $(conf.newNoteTextSelector).val(""); },
                always: null
            }
        ]);

        function confirmDelete(event) {
            const noteName = $(event.target).parent().find("a[note-name]").text();
            return confirm("Delete " + $.trim(noteName) + "?");
        }
    };
})(window.pim, window, jQuery);