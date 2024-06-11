const handleGetActiveMenuSideBar = () => {
    var [key, value] = $(location).attr('href').split('?')[1].split('=');

    ["CM", "P", "USER", "ROLE", "ODER", "INFO"].forEach((menuKey) => {
        let menuKeyOperator = menuKey;
        if (key === "menuKey" && value.endsWith("#")) {
            menuKeyOperator = `${menuKey}#`;
        }

        if (menuKeyOperator === value) {
            const element = $(`.admin-side-bar-menu-item-control[data-menu-key='${menuKey}']`);
            $(element).addClass("active");
            $(element).next(".admin-side-bar-menu-item-child").addClass("active");
        }
    });
};

$(document).ready(function () {
    const handleToggleMultipleMenu = () => {
        $(".admin-side-bar-menu-item-control").on("click", function () {
            $(this).toggleClass("active");
            $(this).next(".admin-side-bar-menu-item-child").toggleClass("active");
        });
    };

    function init() {
        handleToggleMultipleMenu();
    }

    init();
});
