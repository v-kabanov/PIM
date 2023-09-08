(function ($pim, window, $) {
    $pim.pages.latestAndCreate = {};
    const conf = $pim.config;
    const page = $pim.pages.latestAndCreate;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            pim.features.autoAjax.init([
                {
                    url: conf.deleteNoteUrl,
                    triggers: [
                        {
                            selector: conf.deleteNoteButtonSelector,
                            eventName: "click"
                        }],
                    replacementSourceSelector: conf.divNoteListSelector,
                    replacementTargetSelector: conf.divNoteListSelector,
                    confirmFunction: confirmDelete,
                    always: setupComponents,
                    getPostData: function(event) {
                        const target = $(event.target || event.srcElement);
                        const data = [{
                            name: conf.buttonAttributeNameNoteId,
                            value: target.attr(conf.buttonAttributeNameNoteId)
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
                    always: setupComponents
                }
            ]);

            setupComponents();
        });

        function setupComponents() {
        }

        function confirmDelete(event) {
            const noteName = $(event.target).parent().siblings("div[note-name]").find("a[note-name]").text();
            return confirm("Delete " + $.trim(noteName) + "?");
        }
    };
})(pim, window, jQuery);