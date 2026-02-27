window.themeInterop = {
    getTheme: function () {
        return localStorage.getItem("darkMode") === "true";
    },
    setTheme: function (value) {
        localStorage.setItem("darkMode", value.toString());
        if (value) {
            document.documentElement.setAttribute("data-theme", "dark");
        } else {
            document.documentElement.removeAttribute("data-theme");
        }
    }
};
