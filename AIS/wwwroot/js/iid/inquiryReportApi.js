(function (window) {
    function iidPost(path, payload, isForm) {
        var url = (window.g_asiBaseURL || '') + path;
        return $.ajax({
            url: url,
            method: 'POST',
            data: payload,
            processData: !isForm,
            contentType: isForm ? false : 'application/json; charset=utf-8',
            dataType: 'json'
        });
    }

    function iidPostJson(path, payload) {
        return iidPost(path, JSON.stringify(payload || {}), false);
    }

    window.iidGetInqAccusations = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqAccusations', { complaintId: complaintId }); };
    window.iidAddInqAccusation = function (payload) { return iidPostJson('/ApiCalls/AddIidInqAccusation', payload); };
    window.iidUpdateInqAccusation = function (payload) { return iidPostJson('/ApiCalls/UpdateIidInqAccusation', payload); };
    window.iidDeleteInqAccusation = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqAccusation', { id: id, userId: userId }); };

    window.iidGetInqAccusedList = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqAccusedList', { complaintId: complaintId }); };
    window.iidAddInqAccused = function (payload) { return iidPostJson('/ApiCalls/AddIidInqAccused', payload); };
    window.iidUpdateInqAccused = function (payload) { return iidPostJson('/ApiCalls/UpdateIidInqAccused', payload); };
    window.iidDeleteInqAccused = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqAccused', { id: id, userId: userId }); };

    window.iidGetInqRecords = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqRecords', { complaintId: complaintId }); };
    window.iidAddInqRecord = function (payload) { return iidPostJson('/ApiCalls/AddIidInqRecord', payload); };
    window.iidUpdateInqRecord = function (payload) { return iidPostJson('/ApiCalls/UpdateIidInqRecord', payload); };
    window.iidDeleteInqRecord = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqRecord', { id: id, userId: userId }); };

    window.iidGetInqStatements = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqStatements', { complaintId: complaintId }); };
    window.iidAddInqStatement = function (payload) { return iidPostJson('/ApiCalls/AddIidInqStatement', payload); };
    window.iidUpdateInqStatement = function (payload) { return iidPostJson('/ApiCalls/UpdateIidInqStatement', payload); };
    window.iidDeleteInqStatement = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqStatement', { id: id, userId: userId }); };

    window.iidGetInqEvidenceFiles = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqEvidenceFiles', { complaintId: complaintId }); };
    window.iidAddInqEvidenceFile = function (formData) { return iidPost('/ApiCalls/AddIidInqEvidenceFile', formData, true); };
    window.iidDeleteInqEvidenceFile = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqEvidenceFile', { id: id, userId: userId }); };

    window.iidGetInqViolations = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqViolations', { complaintId: complaintId }); };
    window.iidAddInqViolation = function (payload) { return iidPostJson('/ApiCalls/AddIidInqViolation', payload); };
    window.iidUpdateInqViolation = function (payload) { return iidPostJson('/ApiCalls/UpdateIidInqViolation', payload); };
    window.iidDeleteInqViolation = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqViolation', { id: id, userId: userId }); };

    window.iidGetInqDsa = function (complaintId) { return iidPostJson('/ApiCalls/GetIidInqDsa', { complaintId: complaintId }); };
    window.iidAddInqDsa = function (payload) { return iidPostJson('/ApiCalls/AddIidInqDsa', payload); };
    window.iidUpdateInqDsa = function (payload) { return iidPostJson('/ApiCalls/UpdateIidInqDsa', payload); };
    window.iidDeleteInqDsa = function (id, userId) { return iidPostJson('/ApiCalls/DeleteIidInqDsa', { id: id, userId: userId }); };
}(window));
