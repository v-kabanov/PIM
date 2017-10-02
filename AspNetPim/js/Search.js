(function ($pim, window, $) {
    $pim.pages.search = {};
    var conf = $pim.config;
    var page = $pim.pages.search;

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
                    getPostData: function (event) {
                        var target = $(event.target || event.srcElement);
                        var data = $(this.formSelector).serializeArray();
                        data.push({ name: conf.buttonAttributeNameNoteId, value: target.attr(conf.buttonAttributeNameNoteId) });
                        var postData = $.param(data);
                        return postData;
                    },
                }
            ]);

            setupComponents();
        });

        function setupComponents() {
            $(".datepicker").datepicker({dateFormat: "dd M yy"});
        }

        function confirmDelete(event) {
            var noteName = $(event.target).parent().siblings("div[note-name]").find("a[note-name]").text();
            return confirm("Delete " + $.trim(noteName) + "?");
        }
    };
})(pim, window, jQuery);