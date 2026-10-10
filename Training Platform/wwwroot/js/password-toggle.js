(function () {
    "use strict";

    var SVG_ATTRS = 'xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" ' +
        'stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"';

    var ICON_SHOW = '<svg class="pwd-icon-on" ' + SVG_ATTRS + '>' +
        '<path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12Z"></path>' +
        '<circle cx="12" cy="12" r="3"></circle></svg>';

    var ICON_HIDE = '<svg class="pwd-icon-off" ' + SVG_ATTRS + '>' +
        '<path d="M10.6 6.2A9.9 9.9 0 0 1 12 6c6 0 9.5 6 9.5 6a17 17 0 0 1-3.1 3.9"></path>' +
        '<path d="M6.2 6.6A17.2 17.2 0 0 0 2.5 12S6 18 12 18a9.6 9.6 0 0 0 4-.85"></path>' +
        '<path d="M9.9 9.9a3 3 0 0 0 4.2 4.2"></path>' +
        '<path d="M3 3l18 18"></path></svg>';

    function getLabels() {
        var cfg = document.getElementById("pwdToggleConfig");
        return {
            show: (cfg && cfg.getAttribute("data-show")) || "Show password",
            hide: (cfg && cfg.getAttribute("data-hide")) || "Hide password"
        };
    }

    function wrapInput(input) {
        var parent = input.parentNode;
        if (!parent || parent.classList.contains("pwd-field")) {
            return;
        }
        var field = document.createElement("div");
        field.className = "pwd-field";
        parent.insertBefore(field, input);
        field.appendChild(input);
    }

    function enhance(input, labels) {
        if (!input || input.getAttribute("data-pwd-toggle") === "on") {
            return;
        }
        input.setAttribute("data-pwd-toggle", "on");
        wrapInput(input);

        var btn = document.createElement("button");
        btn.type = "button";
        btn.className = "pwd-toggle-btn";
        btn.innerHTML = ICON_SHOW + ICON_HIDE;
        btn.setAttribute("title", labels.show);
        btn.setAttribute("aria-label", labels.show);
        btn.setAttribute("aria-pressed", "false");

        if (input.id) {
            btn.setAttribute("aria-controls", input.id);
        }

        btn.addEventListener("click", function () {
            var willShow = input.type === "password";
            input.type = willShow ? "text" : "password";
            btn.classList.toggle("is-visible", willShow);
            btn.setAttribute("aria-pressed", willShow ? "true" : "false");
            btn.setAttribute("title", willShow ? labels.hide : labels.show);
            btn.setAttribute("aria-label", willShow ? labels.hide : labels.show);

            var len = input.value.length;
            input.focus({ preventScroll: true });
            try {
                input.setSelectionRange(len, len);
            } catch (e) {
                /* not all input types support selection */
            }
        });

        input.parentNode.appendChild(btn);
    }

    function init() {
        var labels = getLabels();
        var inputs = document.querySelectorAll('input[type="password"]');
        for (var i = 0; i < inputs.length; i++) {
            enhance(inputs[i], labels);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
