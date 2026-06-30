$(document).ready(function () {
    addOrnamentRow();
    addProofRow();

    $("#addOrnament").click(function () {
        addOrnamentRow();
    });

    $("#addProof").click(function () {
        addProofRow();
    });

    $(document).on("click", ".remove-ornament", function () {
        $(this).closest("tr").remove();
        refreshIndexes("ornament");
        toggleEmptyState("ornament");
    });

    $(document).on("click", ".remove-proof", function () {
        $(this).closest("tr").remove();
        refreshIndexes("proof");
        toggleEmptyState("proof");
    });

    function addOrnamentRow() {
        removeEmptyState("ornament");
        let index = $("#ornamentTable tbody tr.data-row").length;

        let row = `
        <tr class="data-row">
            <td><input type="file" name="OrnamentFiles" class="form-control" accept="image/*" /></td>
            <td><input type="text" name="Ornaments[${index}].grams" class="form-control" placeholder="e.g. 12.5" /></td>
            <td><input type="text" name="Ornaments[${index}].purity" class="form-control" placeholder="e.g. 22K" /></td>
            <td><input type="text" name="Ornaments[${index}].buyingPrice" class="form-control" placeholder="e.g. 75000" /></td>
            <td><button type="button" class="btn btn-danger btn-remove remove-ornament">X</button></td>
        </tr>`;

        $("#ornamentTable tbody").append(row);
    }

    function addProofRow() {
        removeEmptyState("proof");
        let index = $("#proofTable tbody tr.data-row").length;

        let row = `
        <tr class="data-row">
            <td><input type="text" name="Proofs[${index}].DocumentName" class="form-control" placeholder="Aadhaar / PAN / Passbook" /></td>
            <td><input type="file" name="ProofFiles" class="form-control" /></td>
            <td><button type="button" class="btn btn-danger btn-remove remove-proof">X</button></td>
        </tr>`;

        $("#proofTable tbody").append(row);
    }

    function refreshIndexes(type) {
        let tableSelector = type === "ornament" ? "#ornamentTable" : "#proofTable";
        let groupName = type === "ornament" ? "Ornaments" : "Proofs";

        $(`${tableSelector} tbody tr.data-row`).each(function (i) {
            $(this).find("input[type='text']").each(function () {
                let name = $(this).attr("name");
                if (name && name.indexOf(groupName) === 0) {
                    name = name.replace(/\[\d+\]/, `[${i}]`);
                    $(this).attr("name", name);
                }
            });
        });
    }

    function removeEmptyState(type) {
        let tableSelector = type === "ornament" ? "#ornamentTable" : "#proofTable";
        $(`${tableSelector} .empty-state-row`).remove();
    }

    function toggleEmptyState(type) {
        let tableSelector = type === "ornament" ? "#ornamentTable" : "#proofTable";
        let colspan = type === "ornament" ? 5 : 3;
        let message = type === "ornament"
            ? "No ornaments added. Click Add Ornament to begin."
            : "No proof documents added. Click Add Proof to begin.";

        if ($(`${tableSelector} tbody tr.data-row`).length === 0) {
            $(`${tableSelector} tbody`).append(`
                <tr class="empty-state-row">
                    <td colspan="${colspan}" class="empty-row">${message}</td>
                </tr>`);
        }
    }
});
