(function (window) {
    function iidValue(raw) {
        if (raw === null || raw === undefined) {
            return '';
        }
        return String(raw).trim();
    }

    function iidPick(raw) {
        var keys = Array.prototype.slice.call(arguments, 1);
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            if (raw && raw[key] !== undefined && raw[key] !== null && String(raw[key]).trim() !== '') {
                return raw[key];
            }
        }
        return '';
    }

    function iidNormalizeComplaint(raw) {
        raw = raw || {};
        return {
            ComplaintId: iidPick(raw, 'complaintId', 'ComplaintId', 'COMPLAINT_ID'),
            ComplaintNo: iidPick(raw, 'complaintNo', 'ComplaintNo', 'COMPLAINT_NO'),
            Nature: iidPick(raw, 'nature', 'Nature'),
            Category: iidPick(raw, 'category', 'Category'),
            Contents: iidPick(raw, 'contents', 'Contents'),
            ActionRequired: iidPick(raw, 'actionRequired', 'ActionRequired'),
            UploadedComplaint: iidPick(raw, 'uploadedComplaint', 'UploadedComplaint', 'UPLOADED_COMPLAINT'),
            UploadedFFR: iidPick(raw, 'UploadedFFR', 'uploaded_FFR', 'uploaded_ffr', 'UPLOADED_FFR'),
            UploadedEvidence: iidPick(raw, 'uploadedEvidence', 'UploadedEvidence', 'UPLOADED_EVIDENCE'),
            SubmittedOn: iidPick(raw, 'submittedOn', 'SubmittedOn'),
            Status: iidPick(raw, 'status', 'Status'),
            AssignedUnit: iidPick(raw, 'assignedUnit', 'AssignedUnit', 'assignedUnitId', 'AssignedUnitId'),
            Assessment: iidPick(raw, 'assessment', 'Assessment'),
            Recommendation: iidPick(raw, 'recommendation', 'Recommendation'),
            ComplainantName: iidPick(raw, 'complainantName', 'ComplainantName'),
            Cnic: iidPick(raw, 'cnic', 'Cnic', 'CNIC'),
            CellularNumber: iidPick(raw, 'cellularNumber', 'CellularNumber'),
            MailingAddress: iidPick(raw, 'mailingAddress', 'MailingAddress'),
            Gender: iidPick(raw, 'gender', 'Gender'),
            ReceivedFrom: iidPick(raw, 'receivedFrom', 'ReceivedFrom'),
            LocationType: iidPick(raw, 'locationTypeText', 'locationTypeId', 'LocationTypeText', 'LocationTypeId'),
            GmOffice: iidPick(raw, 'gmOffice', 'gmOfficeId', 'GmOffice', 'GmOfficeId'),
            Region: iidPick(raw, 'region', 'regionId', 'Region', 'RegionId'),
            Branch: iidPick(raw, 'branch', 'branchId', 'Branch', 'BranchId')
        };
    }

    function iidResolveFileUrl(value, baseUrl) {
        var fileValue = iidValue(value);
        if (!fileValue || fileValue.toUpperCase() === 'N/A') {
            return '';
        }
        if (/^(https?:)?\/\//i.test(fileValue) || fileValue.charAt(0) === '/') {
            return fileValue;
        }
        return (baseUrl || '').replace(/\/$/, '') + '/' + encodeURIComponent(fileValue);
    }

    function iidFileCell(value, baseUrl) {
        var fileValue = iidValue(value);
        if (!fileValue || fileValue.toUpperCase() === 'N/A') {
            return '<span class="text-muted">N/A</span>';
        }

        if (fileValue.indexOf(';') > -1) {
            var links = fileValue.split(';').filter(function (x) { return iidValue(x); }).map(function (x) {
                var href = iidResolveFileUrl(x, baseUrl);
                return href ? '<a href="' + href + '" target="_blank">View</a>' : '<span class="text-muted">N/A</span>';
            });
            return links.length ? links.join('<br/>') : '<span class="text-muted">N/A</span>';
        }

        var href = iidResolveFileUrl(fileValue, baseUrl);
        return href ? '<a href="' + href + '" target="_blank">View</a>' : '<span class="text-muted">N/A</span>';
    }

    function iidTextCell(value) {
        var text = iidValue(value);
        return text || 'N/A';
    }

    function iidRenderComplaintDetails(containerSelector, complaint) {
        var c = complaint || {};
        var uploadBase = (window.g_asiBaseURL || '') + '/Uploads';
        var html = '' +
            '<div class="row g-3 mb-3">' +
            '  <div class="col-12 col-lg-6">' +
            '    <div class="card h-100"><div class="card-header bg-light">Complainant Particulars</div><div class="card-body"><dl class="row mb-0">' +
            '      <dt class="col-md-5">Complainant Name</dt><dd class="col-md-7">' + iidTextCell(c.ComplainantName) + '</dd>' +
            '      <dt class="col-md-5">CNIC</dt><dd class="col-md-7">' + iidTextCell(c.Cnic) + '</dd>' +
            '      <dt class="col-md-5">Cellular Number</dt><dd class="col-md-7">' + iidTextCell(c.CellularNumber) + '</dd>' +
            '      <dt class="col-md-5">Mailing Address</dt><dd class="col-md-7">' + iidTextCell(c.MailingAddress) + '</dd>' +
            '      <dt class="col-md-5">Gender</dt><dd class="col-md-7">' + iidTextCell(c.Gender) + '</dd>' +
            '      <dt class="col-md-5">Received From</dt><dd class="col-md-7">' + iidTextCell(c.ReceivedFrom) + '</dd>' +
            '      <dt class="col-md-5">Location Type</dt><dd class="col-md-7">' + iidTextCell(c.LocationType) + '</dd>' +
            '      <dt class="col-md-5">GM Office</dt><dd class="col-md-7">' + iidTextCell(c.GmOffice) + '</dd>' +
            '      <dt class="col-md-5">Region</dt><dd class="col-md-7">' + iidTextCell(c.Region) + '</dd>' +
            '      <dt class="col-md-5">Branch</dt><dd class="col-md-7">' + iidTextCell(c.Branch) + '</dd>' +
            '    </dl></div></div>' +
            '  </div>' +
            '  <div class="col-12 col-lg-6">' +
            '    <div class="card h-100"><div class="card-header bg-light">Complaint Details</div><div class="card-body"><dl class="row mb-0">' +
            '      <dt class="col-md-5">Complaint ID</dt><dd class="col-md-7">' + iidTextCell(c.ComplaintId) + '</dd>' +
            '      <dt class="col-md-5">Complaint No</dt><dd class="col-md-7">' + iidTextCell(c.ComplaintNo) + '</dd>' +
            '      <dt class="col-md-5">Nature</dt><dd class="col-md-7">' + iidTextCell(c.Nature) + '</dd>' +
            '      <dt class="col-md-5">Category</dt><dd class="col-md-7">' + iidTextCell(c.Category) + '</dd>' +
            '      <dt class="col-md-5">Contents</dt><dd class="col-md-7">' + iidTextCell(c.Contents) + '</dd>' +
            '      <dt class="col-md-5">Action Required</dt><dd class="col-md-7">' + iidTextCell(c.ActionRequired) + '</dd>' +
            '      <dt class="col-md-5">Uploaded Complaint</dt><dd class="col-md-7">' + iidFileCell(c.UploadedComplaint, uploadBase) + '</dd>' +
            '      <dt class="col-md-5">Uploaded FFR</dt><dd class="col-md-7">' + iidFileCell(c.UploadedFFR, uploadBase) + '</dd>' +
            '      <dt class="col-md-5">Uploaded Evidence</dt><dd class="col-md-7">' + iidFileCell(c.UploadedEvidence, uploadBase) + '</dd>' +
            '      <dt class="col-md-5">Submitted On</dt><dd class="col-md-7">' + iidTextCell(c.SubmittedOn) + '</dd>' +
            '      <dt class="col-md-5">Status</dt><dd class="col-md-7">' + iidTextCell(c.Status) + '</dd>' +
            '      <dt class="col-md-5">Assigned Unit</dt><dd class="col-md-7">' + iidTextCell(c.AssignedUnit) + '</dd>' +
            '      <dt class="col-md-5">Assessment</dt><dd class="col-md-7">' + iidTextCell(c.Assessment) + '</dd>' +
            '      <dt class="col-md-5">Recommendation</dt><dd class="col-md-7">' + iidTextCell(c.Recommendation) + '</dd>' +
            '    </dl></div></div>' +
            '  </div>' +
            '</div>';

        $(containerSelector).html(html);
    }

    window.iidNormalizeComplaint = iidNormalizeComplaint;
    window.iidFileCell = iidFileCell;
    window.iidRenderComplaintDetails = iidRenderComplaintDetails;
}(window));
