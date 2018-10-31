$(document).ready(function () {
    if ($(".js-switch-chk")[0]) {
        var elems = Array.prototype.slice.call(document.querySelectorAll('.js-switch-chk'));
        elems.forEach(function (html) {
            var switchery = new Switchery(html, {
                color: '#26B99A'
            });
        });
    }
});