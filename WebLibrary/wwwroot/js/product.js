var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#ProductTable').DataTable({
        "ajax": { url: '/product/ProductList' },
        "columns": [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div style="display:flex; justify-content:flex-end; gap:6px;">
                        <a href="/product/Edit?id=${data}" class="lib-icon-btn edit" title="Edit">
                            <i class="ti ti-pencil" style="font-size:15px;"></i>
                        </a>
                        <a onClick="Delete('/product/delete/${data}')" class="lib-icon-btn delete" title="Delete">
                            <i class="ti ti-trash" style="font-size:15px;"></i>
                        </a>
                    </div>`;
                },
                "width": "25%"
            }
        ]
    });
}