document.addEventListener('DOMContentLoaded', function () {
    const gridViewBtn = document.getElementById('gridView');
    const listViewBtn = document.getElementById('listView');
    const productsContainer = document.querySelector('.col-lg-10 .row');

    function setGridView() {
        productsContainer.classList.remove('list-view');
        productsContainer.classList.add('grid-view');
        gridViewBtn.classList.add('active');
        listViewBtn.classList.remove('active');
    }

    function setListView() {
        productsContainer.classList.remove('grid-view');
        productsContainer.classList.add('list-view');
        listViewBtn.classList.add('active');
        gridViewBtn.classList.remove('active');
    }

    gridViewBtn.addEventListener('click', setGridView);
    listViewBtn.addEventListener('click', setListView);

    // Set default view
    setGridView();
});