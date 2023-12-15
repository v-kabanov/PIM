(function ($pim, window, $) {
    $pim.pages.search = {};
    const conf = $pim.config;
    const page = $pim.pages.search;

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
                confirmFunction: confirmDelete,
                always: setupComponents,
                getPostData: function (event) {
                    const target = $(event.target || event.srcElement);
                    const data = $(this.formSelector).serializeArray();
                    data.push({ name: conf.attributeNameNoteId, value: target.attr(conf.attributeNameNoteId) });
                    const postData = $.param(data);
                    return postData;
                }
            }
        ]);

        setupComponents();

        function setupComponents() {
            $(".datepicker").datepicker({dateFormat: "dd M yy"});
        }

        function confirmDelete(event) {
            const noteName = $(event.target).parent().find("a[note-name]").text();
            return confirm("Delete " + $.trim(noteName) + "?");
        }
    };
})(pim, window, jQuery);