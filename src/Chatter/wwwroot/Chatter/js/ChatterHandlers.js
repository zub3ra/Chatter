(function init() {
    var template = (document._currentScript || document.currentScript).previousElementSibling;

    template.onInputKeyup = function (e) {
        if (e.keyCode == 13) {
            template.set("model.Send$", template.model.Send$ + 1);
        }
    };

    template.clearFoundAttachments = function () {
        setTimeout(function () {
            template.set("model.ClearFoundAttachments$", template.model.ClearFoundAttachments$ + 1);
        }, 100);
    };
})();