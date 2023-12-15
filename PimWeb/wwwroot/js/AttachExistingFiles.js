(function ($pim, window, $) {
    $pim.pages.attachExistingFiles = {};
    const conf = $pim.config;
    const page = $pim.pages.attachExistingFiles;

    page.init = function (configData) {
        $.extend($pim.config, configData);

        $(document).ready(function () {
            pim.features.autoAjax.init([
                {
                    url: conf.attachUrl,
                    triggers: [
                        {
                            selector: conf.btnAttachSelector,
                            eventName: "click"
                        }],
                    confirmFunction: null,
                    always: null,
                    predicate: null,
                    success: null //() => window.location.reload(),
                }
            ]);
        });
    };
})(pim, window, jQuery);