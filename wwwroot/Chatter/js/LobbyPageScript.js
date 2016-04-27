(function () {
    var script = document._currentScript || document.currentScript;
    var template = script.previousElementSibling;

    template.onInputKeyup = function (e) {
        if (e.keyCode == 13) {
            template.set("model.GoToNewGroup$", template.model.GoToNewGroup$ + 1);
        }
    };
})();