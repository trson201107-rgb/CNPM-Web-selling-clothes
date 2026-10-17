(function () {
    'use strict';

    // 1) Hộp thoại xác nhận cho form có thuộc tính data-confirm
    document.addEventListener('submit', function (e) {
        var f = e.target;
        if (f && f.matches && f.matches('form[data-confirm]')) {
            if (!window.confirm(f.getAttribute('data-confirm'))) e.preventDefault();
        }
    }, true);

    // 2) Chọn màu + size ở trang chi tiết sản phẩm
    function initVariantPicker() {
        var form = document.getElementById('addToCartForm');
        if (!form) return;

        var variants = [];
        try { variants = JSON.parse(form.getAttribute('data-variants') || '[]'); } catch (err) { variants = []; }

        var hidden = document.getElementById('maBT');
        var info = document.getElementById('stockInfo');
        var btn = document.getElementById('addBtn');
        var qty = document.getElementById('soLuong');

        function selectedValue(cls) {
            var el = form.querySelector('input.' + cls + ':checked');
            return el ? parseInt(el.value, 10) : 0;
        }
        function find(mau, size) {
            for (var i = 0; i < variants.length; i++) {
                if (variants[i].maMau === mau && variants[i].maSize === size) return variants[i];
            }
            return null;
        }
        function setInfo(text, cls) {
            info.textContent = text;
            info.className = 'small mb-3 ' + cls;
        }

        function refresh() {
            var mau = selectedValue('variant-mau');

            // Vô hiệu các size không có (hoặc hết hàng) ở màu đang chọn
            form.querySelectorAll('input.variant-size').forEach(function (inp) {
                var size = parseInt(inp.value, 10);
                var v = mau ? find(mau, size) : null;
                var label = form.querySelector('label[for="' + inp.id + '"]');
                inp.disabled = !!(mau && !v);
                if (inp.disabled && inp.checked) inp.checked = false;
                if (label) label.classList.toggle('disabled-size', !!(mau && (!v || v.ton <= 0)));
            });

            var size = selectedValue('variant-size');
            if (!mau || !size) {
                hidden.value = '';
                setInfo('Chọn màu và kích cỡ để xem tồn kho.', 'text-muted');
                if (btn) btn.disabled = true;
                return;
            }
            var v2 = find(mau, size);
            if (!v2) {
                hidden.value = '';
                setInfo('Không có biến thể này.', 'text-danger');
                if (btn) btn.disabled = true;
                return;
            }
            hidden.value = v2.maBT;
            if (v2.ton <= 0) {
                setInfo('Hết hàng', 'text-danger');
                if (btn) btn.disabled = true;
            } else {
                setInfo('Còn ' + v2.ton + ' sản phẩm' + (v2.ton <= 5 ? ' (sắp hết)' : ''), v2.ton <= 5 ? 'text-warning-emphasis' : 'text-success');
                if (btn) btn.disabled = false;
                if (qty) {
                    qty.max = Math.min(99, v2.ton);
                    if (parseInt(qty.value, 10) > v2.ton) qty.value = v2.ton;
                }
            }
        }

        form.addEventListener('change', refresh);

        // Chỉ có một màu và một size thì chọn sẵn
        var maus = form.querySelectorAll('input.variant-mau');
        var sizes = form.querySelectorAll('input.variant-size');
        if (maus.length === 1) maus[0].checked = true;
        if (sizes.length === 1) sizes[0].checked = true;
        refresh();
    }

    // 3) Bảng biến thể trong form thêm/sửa sản phẩm
    function initVariantRows() {
        var tbody = document.getElementById('variantRows');
        var tpl = document.getElementById('variantTemplate');
        var addBtn = document.getElementById('addVariantBtn');
        var form = document.getElementById('productForm');
        if (!tbody || !tpl || !addBtn || !form) return;

        var next = tbody.querySelectorAll('.variant-row').length;

        addBtn.addEventListener('click', function () {
            tbody.insertAdjacentHTML('beforeend', tpl.innerHTML.replace(/__I__/g, String(next++)));
        });

        tbody.addEventListener('click', function (e) {
            var b = e.target.closest('.js-remove-row');
            if (!b) return;
            var row = b.closest('tr');
            if (row && row.parentNode) row.parentNode.removeChild(row);
        });

        // Đánh lại chỉ số 0,1,2... trước khi gửi để máy chủ nhận đủ danh sách
        form.addEventListener('submit', function () {
            tbody.querySelectorAll('.variant-row').forEach(function (row, idx) {
                row.querySelectorAll('[name]').forEach(function (el) {
                    el.name = el.name.replace(/BienThes\[\d+\]/, 'BienThes[' + idx + ']');
                });
            });
        });
    }

    // 4) Tự đóng thông báo thành công sau 5 giây
    function initAlerts() {
        setTimeout(function () {
            document.querySelectorAll('.alert-success.alert-dismissible').forEach(function (el) {
                el.classList.remove('show');
                setTimeout(function () { if (el.parentNode) el.parentNode.removeChild(el); }, 300);
            });
        }, 5000);
    }

    document.addEventListener('DOMContentLoaded', function () {
        initVariantPicker();
        initVariantRows();
        initAlerts();
    });
})();
