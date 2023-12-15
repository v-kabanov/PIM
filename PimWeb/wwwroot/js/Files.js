(function ($pim, window, $) {
    $pim.pages.files = {};
    const conf = $pim.config;
    const page = $pim.pages.files;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            pim.features.autoAjax.init([
                {
                    url: conf.deleteUrl,
                    triggers: [
                        {
                            selector: conf.deleteSelector,
                            eventName: "click"
                        }],
                    confirmFunction: confirmDelete,
                    always: null,
                    predicate: null,
                    success: function () {
                        window.location.reload();
                    },
                    getPostData: function (event) {
                        const target = $(event.target || event.srcElement);
                        const data = { id: target.attr(conf.attributeNameFileId) };
                        const postData = $.param(data);
                        return postData;
                    }
                }
            ]);
        });

        function confirmDelete(e) {
            return confirm("Delete this file permanently?");
        }
    };
})(pim, window, jQuery);