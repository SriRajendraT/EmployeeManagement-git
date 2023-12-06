function confirmDelete(uid, isdelclicked) {
    var deleteSpan = 'deleteSpan_' + uid;

    var confirmDeleteSpan = 'confirmDeleteSpan_' + uid;
 
    if (isdelclicked) {
        $('#'+ deleteSpan).hide();
        $('#'+ confirmDeleteSpan).show();
    } else {
        $('#'+ deleteSpan).show();
        $('#'+ confirmDeleteSpan).hide();
    }
}

function confirmRoleDelete(uid, isdelclicked) {
    var deleteSpan = 'deleteRoleSpan_' + uid;
    
    var confirmDeleteSpan = 'confirmDeleteRoleSpan_' + uid;

    if (isdelclicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}