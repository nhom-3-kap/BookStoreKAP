const handleGetActiveMenuSideBar = () => {
    const currentHref = $(location).attr('href');
    const currentHrefSplit = currentHref.split('?');
    let hrefValue = [];
    if (currentHrefSplit[1]) {
        hrefValue = currentHrefSplit[1].split('=');
        hrefValue[1] = hrefValue[1].split('&')[0];
    }
    var [key, value] = hrefValue;
    if (key === "menuKey") {
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
    }
};
handleGetActiveMenuSideBar();

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
