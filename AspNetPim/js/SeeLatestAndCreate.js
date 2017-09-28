(function ($pim, window, $) {
    $pim.pages.latestAndCreate = {};
    var conf = $pim.config;
    var page = $pim.pages.latestAndCreate;

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
                        var target = $(event.target || event.srcElement);
                        var data = [{ name: conf.buttonAttributeNameNoteId, value: target.attr(conf.buttonAttributeNameNoteId) }];
                        var postData = $.param(data);
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
            var noteName = $(event.target).parent().siblings("div[note-name]").find("a[note-name]").text();
            return confirm("Delete " + $.trim(noteName) + "?");
        }

        function trimInputCallback() {
            pim.features.elementHelper.trimInput(this);
        }
    };
})(pim, window, jQuery);