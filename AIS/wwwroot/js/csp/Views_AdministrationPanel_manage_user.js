var g_userList = [];
var g_userId = 0;
var g_userContexts = [];

$(document).ready(function () {
    if ($('#roleSaveField').length && $.fn.select2) {
        $('#roleSaveField').select2();
        $('#roleSaveField').css('width', '100%');
    }

    $('#isActiveSaveField').prop('checked', true);
    $('#addAssignmentButton').on('click', addAssignmentButtonClickHandler);
});

function normalizeFlag(value, defaultValue) {
    var flag = (value || defaultValue || 'N').toString().trim().toUpperCase();
    return flag === 'Y' ? 'Y' : 'N';
}

function normalizeOptionalText(value) {
    var text = (value || '').toString().trim();
    return text === '' ? null : text;
}

function normalizeOptionalDateValue(value) {
    var text = normalizeOptionalText(value);
    return text || null;
}

function escapeHtml(value) {
    return $('<div>').text(value || '').html();
}

function getOptionText(selector) {
    return ($(selector).text() || '').trim();
}

function toInt(value) {
    var parsed = parseInt(value, 10);
    return isNaN(parsed) ? 0 : parsed;
}

function uniqueJoin(values) {
    var seen = {};
    var result = [];
    $.each(values || [], function (_, value) {
        var text = (value || '').toString().trim();
        var key = text.toLowerCase();
        if (!text || seen[key]) {
            return;
        }

        seen[key] = true;
        result.push(text);
    });

    return result.join(', ');
}

function mapApiContext(context) {
    if (!context) {
        return null;
    }

    return {
        assignmentId: toInt(context.assignmentId || context.userContextId || context.userContextAssignmentId || context.ASSIGNMENT_ID || context.USER_CONTEXT_ID),
        roleId: toInt(context.roleId || context.groupId || context.ROLE_ID || context.GROUP_ID),
        groupId: toInt(context.groupId || context.roleId || context.GROUP_ID || context.ROLE_ID),
        roleName: (context.roleName || context.userRoleName || context.GROUP_NAME || '').toString(),
        entityId: toInt(context.entityId || context.ENTITY_ID),
        entityName: (context.entityName || context.userEntityName || context.ENT_NAME || '').toString(),
        parentEntityId: toInt(context.parentEntityId || context.PARENT_ENTITY_ID),
        parentEntityName: (context.parentEntityName || context.userParentEntityName || context.PARENT_ENTITY_NAME || '').toString(),
        relationshipTypeId: toInt(context.relationshipTypeId || context.RELATIONSHIP_TYPE_ID),
        relationshipTypeName: (context.relationshipTypeName || context.RELATIONSHIP_TYPE_NAME || '').toString(),
        isDefault: normalizeFlag(context.isDefault || context.IS_DEFAULT || context.ISDEFAULT, 'N'),
        isActive: normalizeFlag(context.isActive || context.IS_ACTIVE || context.ISACTIVE, 'Y'),
        assignmentType: normalizeOptionalText(context.assignmentType || context.ASSIGNMENT_TYPE) || 'MANUAL',
        effectiveFrom: normalizeOptionalDateValue(context.effectiveFrom || context.EFFECTIVE_FROM),
        effectiveTo: normalizeOptionalDateValue(context.effectiveTo || context.EFFECTIVE_TO),
        remarks: normalizeOptionalText(context.remarks || context.REMARKS),
        isDeleted: !!(context.isDeleted || context.ISDELETED)
    };
}

function buildLegacyContextFromUser(user) {
    return {
        assignmentId: 0,
        roleId: toInt(user.userRoleID || user.userGroupID),
        groupId: toInt(user.userGroupID || user.userRoleID),
        roleName: (user.userRole || user.userGroup || '').toString(),
        entityId: toInt(user.userEntityID),
        entityName: (user.userEntityName || '').toString(),
        parentEntityId: toInt(user.userParentEntityID),
        parentEntityName: (user.userParentEntityName || '').toString(),
        relationshipTypeId: toInt(user.relationshipId),
        relationshipTypeName: '',
        isDefault: 'Y',
        isActive: normalizeFlag(user.isActive, 'Y'),
        assignmentType: 'MANUAL',
        effectiveFrom: null,
        effectiveTo: null,
        remarks: null,
        isDeleted: false
    };
}

function mergeUserRows(users) {
    var grouped = {};

    $.each(users || [], function (_, user) {
        var key = (user.id || 0) > 0 ? user.id.toString() : (user.ppNumber || ('row_' + Math.random()));
        if (!grouped[key]) {
            grouped[key] = $.extend({}, user);
            grouped[key]._roles = [];
            grouped[key]._entities = [];
            grouped[key]._parents = [];
            grouped[key]._contextKeys = {};
        }

        var current = grouped[key];
        current._roles.push(user.userRole);
        current._entities.push(user.userEntityName);
        current._parents.push(user.userParentEntityName);

        var contextKey = [
            toInt(user.userRoleID || user.userGroupID),
            toInt(user.userEntityID),
            toInt(user.relationshipId),
            toInt(user.userParentEntityID)
        ].join(':');
        current._contextKeys[contextKey] = true;
    });

    return $.map(grouped, function (user) {
        user.userRole = uniqueJoin(user._roles) || user.userRole;
        user.userGroup = user.userRole;
        user.userEntityName = uniqueJoin(user._entities) || user.userEntityName;
        user.userParentEntityName = uniqueJoin(user._parents) || user.userParentEntityName;
        user.assignmentCount = Math.max(Object.keys(user._contextKeys || {}).length, user.assignmentCount || 1);
        delete user._roles;
        delete user._entities;
        delete user._parents;
        delete user._contextKeys;
        return user;
    });
}

function renderUserList(users) {
    var $tbody = $('#userListTable tbody');
    $tbody.empty();

    if (!users || users.length === 0) {
        $tbody.append('<tr><td colspan="9" class="text-center text-muted">No users matched the provided criteria.</td></tr>');
        return;
    }

    $.each(users, function (_, user) {
        $tbody.append(
            '<tr id="userrecordrow_' + user.id + '">' +
            '<td>' + escapeHtml(user.name) + '</td>' +
            '<td>' + escapeHtml(user.ppNumber) + '</td>' +
            '<td>' + escapeHtml(user.userParentEntityName) + '</td>' +
            '<td>' + escapeHtml(user.userEntityName) + '</td>' +
            '<td>' + escapeHtml(user.email) + '</td>' +
            '<td>' + escapeHtml(user.userRole) + '</td>' +
            '<td>' + escapeHtml((user.assignmentCount || 1).toString()) + '</td>' +
            '<td>' + escapeHtml(user.isActive) + '</td>' +
            '<td><a class="text-danger editmode" href="#" data-onclick="event.preventDefault();editUserDetails(' + user.id + ')">Edit</a><a class="text-danger cancelmode d-none" href="#" data-onclick="cancelEditUserDetails(' + user.id + ')">Cancel</a></td>' +
            '</tr>'
        );
    });
}

function normalizeContextCollection(preferredDefaultIndex) {
    $.each(g_userContexts, function (index, context) {
        if (!context) {
            return;
        }

        context.assignmentId = toInt(context.assignmentId);
        context.roleId = toInt(context.roleId);
        context.groupId = toInt(context.groupId || context.roleId);
        context.entityId = toInt(context.entityId);
        context.parentEntityId = toInt(context.parentEntityId);
        context.relationshipTypeId = toInt(context.relationshipTypeId);
        context.isDeleted = !!context.isDeleted;
        context.isDefault = context.isDeleted ? 'N' : normalizeFlag(context.isDefault, 'N');
        context.isActive = context.isDeleted ? 'N' : normalizeFlag(context.isActive, 'Y');
        context.assignmentType = normalizeOptionalText(context.assignmentType) || 'MANUAL';
        context.effectiveFrom = normalizeOptionalDateValue(context.effectiveFrom);
        context.effectiveTo = normalizeOptionalDateValue(context.effectiveTo);
        context.remarks = normalizeOptionalText(context.remarks);
    });

    if (typeof preferredDefaultIndex === 'number' && preferredDefaultIndex >= 0 && g_userContexts[preferredDefaultIndex] && !g_userContexts[preferredDefaultIndex].isDeleted) {
        $.each(g_userContexts, function (index, context) {
            if (!context || context.isDeleted) {
                return;
            }

            context.isDefault = index === preferredDefaultIndex ? 'Y' : 'N';
        });
    }
}

function renderUserContexts() {
    var $tbody = $('#userContextTableBody');
    $tbody.empty();

    var hasVisibleContexts = false;
    $.each(g_userContexts, function (index, context) {
        if (!context || context.isDeleted) {
            return;
        }

        hasVisibleContexts = true;
        var isDefaultChecked = context.isDefault === 'Y' ? 'checked="checked"' : '';
        var isActiveChecked = context.isActive === 'Y' ? 'checked="checked"' : '';

        $tbody.append(
            '<tr data-context-index="' + index + '">' +
            '<td class="text-center"><input type="radio" name="defaultUserContext" ' + isDefaultChecked + ' onclick="setDefaultContext(' + index + ')" /></td>' +
            '<td class="text-center"><input type="checkbox" ' + isActiveChecked + ' onclick="toggleContextActive(' + index + ', this.checked)" /></td>' +
            '<td>' + escapeHtml(context.roleName) + '</td>' +
            '<td>' + escapeHtml(context.parentEntityName) + '</td>' +
            '<td>' + escapeHtml(context.entityName) + '</td>' +
            '<td>' + escapeHtml(context.relationshipTypeName) + '</td>' +
            '<td class="text-center"><a href="#" onclick="event.preventDefault();removeUserContext(' + index + ')">Remove</a></td>' +
            '</tr>'
        );
    });

    if (!hasVisibleContexts) {
        $tbody.append('<tr><td colspan="7" class="text-center text-muted">No role and posting assignments added yet.</td></tr>');
    }
}

function resetUserContexts() {
    g_userContexts = [];
    renderUserContexts();
}

function findContextIndex(candidate, includeDeleted) {
    var matchIndex = -1;

    $.each(g_userContexts, function (index, context) {
        if (!context || (!includeDeleted && context.isDeleted)) {
            return;
        }

        if (candidate.assignmentId > 0 && context.assignmentId > 0 && candidate.assignmentId === context.assignmentId) {
            matchIndex = index;
            return false;
        }

        if (context.roleId === candidate.roleId &&
            context.entityId === candidate.entityId &&
            context.parentEntityId === candidate.parentEntityId &&
            context.relationshipTypeId === candidate.relationshipTypeId) {
            matchIndex = index;
            return false;
        }
    });

    return matchIndex;
}

function buildCurrentContext() {
    var roleId = toInt($('#roleSaveField option:selected').val());
    var entityId = toInt($('#childposting option:selected').val());
    if (roleId <= 0 || entityId <= 0) {
        return null;
    }

    return {
        assignmentId: 0,
        roleId: roleId,
        groupId: roleId,
        roleName: getOptionText('#roleSaveField option:selected'),
        entityId: entityId,
        entityName: getOptionText('#childposting option:selected'),
        parentEntityId: toInt($('#controlingsearch option:selected').val()),
        parentEntityName: getOptionText('#controlingsearch option:selected'),
        relationshipTypeId: toInt($('#RelationshipField option:selected').val()),
        relationshipTypeName: getOptionText('#RelationshipField option:selected'),
        isDefault: $('#defaultAssignmentSaveField').is(':checked') ? 'Y' : 'N',
        isActive: $('#isActiveSaveField').is(':checked') ? 'Y' : 'N',
        assignmentType: 'MANUAL',
        effectiveFrom: null,
        effectiveTo: null,
        remarks: null,
        isDeleted: false
    };
}

function upsertContext(candidate) {
    var existingIndex = findContextIndex(candidate, false);
    if (existingIndex < 0) {
        existingIndex = findContextIndex(candidate, true);
    }

    if (existingIndex >= 0) {
        var existing = g_userContexts[existingIndex];
        candidate.assignmentId = existing.assignmentId || candidate.assignmentId;
        candidate.isDeleted = false;
        g_userContexts[existingIndex] = candidate;
    } else {
        g_userContexts.push(candidate);
        existingIndex = g_userContexts.length - 1;
    }

    normalizeContextCollection(candidate.isDefault === 'Y' ? existingIndex : null);
    renderUserContexts();
}

function addAssignmentButtonClickHandler() {
    var candidate = buildCurrentContext();
    if (!candidate) {
        alert('Select one role and one place of posting before adding an assignment.');
        return;
    }

    upsertContext(candidate);
}

function removeUserContext(index) {
    if (!g_userContexts[index]) {
        return;
    }

    if (g_userContexts[index].assignmentId > 0) {
        g_userContexts[index].isDeleted = true;
        g_userContexts[index].isDefault = 'N';
        g_userContexts[index].isActive = 'N';
    } else {
        g_userContexts.splice(index, 1);
    }

    normalizeContextCollection();
    renderUserContexts();
}

function setDefaultContext(index) {
    normalizeContextCollection(index);
    renderUserContexts();
}

function toggleContextActive(index, isActive) {
    if (!g_userContexts[index]) {
        return;
    }

    g_userContexts[index].isActive = isActive ? 'Y' : 'N';
    if (!isActive && g_userContexts[index].isDefault === 'Y') {
        g_userContexts[index].isDefault = 'N';
    }

    normalizeContextCollection();
    renderUserContexts();
}

function applyContextToBuilder(context) {
    if (!context) {
        return;
    }

    $('#roleSaveField').val(context.groupId || context.roleId).trigger('change');
    $('#defaultAssignmentSaveField').prop('checked', context.isDefault === 'Y');
    $('#isActiveSaveField').prop('checked', context.isActive === 'Y');
    $('#RelationshipField').val(context.relationshipTypeId || 0);

    if (context.relationshipTypeId > 0) {
        getrelation(context.parentEntityId || 0, context.entityId || 0);
    }
}

function loadUserContexts(user) {
    if (!user || !user.id) {
        resetUserContexts();
        return;
    }

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/GetUserContexts',
        type: 'POST',
        data: {
            userId: user.id,
            ppNumber: user.ppNumber
        },
        cache: false,
        success: function (data) {
            g_userContexts = $.map(data || [], function (context) {
                return mapApiContext(context);
            });

            if (!g_userContexts.length) {
                g_userContexts = [buildLegacyContextFromUser(user)];
            }

            normalizeContextCollection();
            renderUserContexts();

            var defaultContext = null;
            $.each(g_userContexts, function (_, context) {
                if (context && !context.isDeleted && context.isDefault === 'Y') {
                    defaultContext = context;
                    return false;
                }
            });

            applyContextToBuilder(defaultContext || g_userContexts[0]);
        },
        error: function (xhr) {
            resetUserContexts();
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), 'Unable to load user assignments.');
        },
        dataType: 'json'
    });
}

function getrelation(parentEntityId, userEntityId) {
    $('#controlingsearch').empty();
    $('#childposting').empty();

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/getparentrel',
        type: 'POST',
        data: {
            ENTITY_REALTION_ID: $('#RelationshipField option:selected').val()
        },
        cache: false,
        success: function (data) {
            $('#controlingsearch').append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
            $.each(data, function (_, contof) {
                var selected = contof.entitY_ID === parentEntityId ? 'selected="selected"' : '';
                $('#controlingsearch').append('<option ' + selected + ' value="' + contof.entitY_ID + '" id="' + contof.entitY_REALTION_ID + '">' + contof.description + '</option>');
            });

            if (userEntityId) {
                getplacepost(userEntityId);
            }
        },
        dataType: 'json'
    });
}

function getplacepost(userEntityId) {
    $('#childposting').empty();

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/getpostplace',
        type: 'POST',
        data: {
            E_R_ID: $('#controlingsearch option:selected').val()
        },
        cache: false,
        success: function (data) {
            $('#childposting').append('<option id="0" value="0" selected="selected">--Select Place of Posting--</option>');
            $.each(data, function (_, gpp) {
                var selected = gpp.entitY_ID === userEntityId ? 'selected="selected"' : '';
                $('#childposting').append('<option ' + selected + ' value="' + gpp.entitY_ID + '" id="' + gpp.entitY_ID + '">' + gpp.c_NAME + '</option>');
            });
        },
        dataType: 'json'
    });
}

function findButtonClickHandler() {
    var whereCheck = 0;
    var entityId = 0;

    if ($('#ppnoSearchField').val() !== '') {
        whereCheck++;
    }
    if ($('#loginSearchField').val() !== '') {
        whereCheck++;
    }
    if ($('#childposting').val() !== '0') {
        whereCheck++;
        entityId = $('#childposting').val();
    } else if ($('#controlingsearch').val() !== '0') {
        whereCheck++;
        entityId = $('#controlingsearch').val();
    }
    if ($('#emailSearchField').val() !== '') {
        whereCheck++;
    }
    if ($('#roleSearchField option:selected').val() !== '0') {
        whereCheck++;
    }

    if (whereCheck === 0) {
        alert('Please provide at least one filter to proceed');
        return;
    }

    $.ajax({
        url: g_asiBaseURL + '/ApiCalls/FindUsers',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            ppNumber: $('#ppnoSearchField').val(),
            loginName: $('#loginSearchField').val(),
            entityId: entityId,
            email: $('#emailSearchField').val(),
            groupId: $('#roleSearchField option:selected').val()
        }),
        cache: false,
        success: function (data) {
            g_userList = mergeUserRows(data || []);
            renderUserList(g_userList);
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), 'Unable to search users.');
        },
        dataType: 'json'
    });
}

function editUserDetails(userId) {
    g_userId = userId;
    $('#userrecordrow_' + userId).css({ background: 'rgba(0,0,0,.05)', 'font-weight': 'bold', color: 'navy' });
    $('#userrecordrow_' + userId + ' .editmode').addClass('d-none');
    $('#userrecordrow_' + userId + ' .cancelmode').removeClass('d-none');
    $('#saveChangesButton').removeClass('d-none');
    $('#addNewUserChangesButton').removeClass('d-none');
    $('#findButton').addClass('d-none');
    $('#searchFieldsContainer').addClass('d-none');
    $('#saveChangesFieldsContainer').removeClass('d-none');

    $.each(g_userList, function (_, user) {
        if (user.id !== userId) {
            return;
        }

        $('#ppnoSaveField').val(user.ppNumber);
        $('#loginSaveField').val(user.ppNumber);
        $('#emailSaveField').val(user.email);
        $('#isActiveSaveField').prop('checked', normalizeFlag(user.isActive, 'Y') === 'Y');
        $('#roleSaveField').val(user.userGroupID || user.userRoleID || 0).trigger('change');
        $('#RelationshipField').val(user.relationshipId || 0);
        getrelation(toInt(user.userParentEntityID), toInt(user.userEntityID));
        loadUserContexts(user);
    });
}

function cancelEditUserDetails(userId) {
    g_userId = 0;
    $('#userrecordrow_' + userId).css({ background: 'none', 'font-weight': 'normal', color: 'black' });
    $('#userrecordrow_' + userId + ' .editmode').removeClass('d-none');
    $('#userrecordrow_' + userId + ' .cancelmode').addClass('d-none');
    $('#saveChangesButton').addClass('d-none');
    $('#addNewUserChangesButton').addClass('d-none');
    $('#findButton').removeClass('d-none');
    $('#saveChangesFieldsContainer').addClass('d-none');
    $('#searchFieldsContainer').removeClass('d-none');
    clearSearchFields();
}

function clearSearchFields() {
    $('#ppnoSaveField').val('');
    $('#loginSaveField').val('');
    $('#emailSaveField').val('');
    $('#passSaveField').val('');
    $('#conPassSaveField').val('');
    $('#roleSaveField').val(0).trigger('change');
    $('#RelationshipField').val(0);
    $('#controlingsearch').empty().append('<option id="0" value="0">--Select Controlling/Reporting Office--</option>');
    $('#childposting').empty().append('<option id="0" value="0">--Select Place of Posting--</option>');
    $('#isActiveSaveField').prop('checked', true);
    $('#defaultAssignmentSaveField').prop('checked', false);
    $('#resetpassword').prop('checked', false);
    resetUserContexts();
}

function buildAssignmentPayloadFromContext(context, isDeleted) {
    return {
        ASSIGNMENT_ID: context.assignmentId > 0 ? context.assignmentId : null,
        USER_CONTEXT_ID: context.assignmentId > 0 ? context.assignmentId : null,
        ROLE_ID: context.roleId > 0 ? context.roleId : null,
        GROUP_ID: context.groupId > 0 ? context.groupId : (context.roleId > 0 ? context.roleId : null),
        ENTITY_ID: context.entityId > 0 ? context.entityId : null,
        ISDEFAULT: isDeleted ? 'N' : normalizeFlag(context.isDefault, 'N'),
        ISACTIVE: isDeleted ? 'N' : normalizeFlag(context.isActive, 'Y'),
        ASSIGNMENT_TYPE: normalizeOptionalText(context.assignmentType) || 'MANUAL',
        EFFECTIVE_FROM: normalizeOptionalDateValue(context.effectiveFrom),
        EFFECTIVE_TO: normalizeOptionalDateValue(context.effectiveTo),
        REMARKS: normalizeOptionalText(context.remarks),
        ISDELETED: !!isDeleted
    };
}

function collectAssignmentsFromGrid() {
    var visibleAssignments = [];
    var deletedAssignments = [];
    var rowCount = 0;

    $('#userContextTableBody tr[data-context-index]').each(function () {
        var contextIndex = toInt($(this).attr('data-context-index'));
        var context = g_userContexts[contextIndex];
        if (!context || context.isDeleted) {
            return;
        }

        rowCount++;
        visibleAssignments.push(buildAssignmentPayloadFromContext(context, false));
    });

    $.each(g_userContexts, function (_, context) {
        if (!context || !context.isDeleted || !(context.assignmentId > 0)) {
            return;
        }

        deletedAssignments.push(buildAssignmentPayloadFromContext(context, true));
    });

    return {
        rowCount: rowCount,
        visibleAssignments: visibleAssignments,
        assignments: deletedAssignments.concat(visibleAssignments)
    };
}

function validateAssignments(collectedAssignments) {
    if (!collectedAssignments || collectedAssignments.rowCount === 0 || !collectedAssignments.visibleAssignments.length) {
        return 'Add at least one role and posting assignment to the grid before saving the user.';
    }

    var duplicateRows = {};
    var activeCount = 0;
    var defaultRows = [];
    var inactiveDefaultRow = 0;

    $.each(collectedAssignments.visibleAssignments, function (index, assignment) {
        var rowNumber = index + 1;

        if (!(assignment.ROLE_ID > 0) || !(assignment.ENTITY_ID > 0)) {
            duplicateRows.invalid = rowNumber;
            return false;
        }

        assignment.GROUP_ID = assignment.GROUP_ID || assignment.ROLE_ID;
        assignment.ISDEFAULT = normalizeFlag(assignment.ISDEFAULT, 'N');
        assignment.ISACTIVE = normalizeFlag(assignment.ISACTIVE, 'Y');
        assignment.ASSIGNMENT_TYPE = normalizeOptionalText(assignment.ASSIGNMENT_TYPE) || 'MANUAL';
        assignment.EFFECTIVE_FROM = normalizeOptionalDateValue(assignment.EFFECTIVE_FROM);
        assignment.EFFECTIVE_TO = normalizeOptionalDateValue(assignment.EFFECTIVE_TO);
        assignment.REMARKS = normalizeOptionalText(assignment.REMARKS);

        if (assignment.ISACTIVE === 'Y') {
            activeCount++;
        }

        if (assignment.ISDEFAULT === 'Y') {
            defaultRows.push(rowNumber);
            if (assignment.ISACTIVE !== 'Y' && !inactiveDefaultRow) {
                inactiveDefaultRow = rowNumber;
            }
        }

        var duplicateKey = [assignment.ROLE_ID, assignment.GROUP_ID, assignment.ENTITY_ID].join(':');
        if (duplicateRows[duplicateKey]) {
            duplicateRows.duplicate = duplicateRows[duplicateKey] + ',' + rowNumber;
            return false;
        }

        duplicateRows[duplicateKey] = rowNumber;
        return true;
    });

    if (duplicateRows.invalid) {
        return 'Assignment row ' + duplicateRows.invalid + ' must include one role and one entity.';
    }

    if (duplicateRows.duplicate) {
        var rows = duplicateRows.duplicate.split(',').join(' and ');
        return 'Duplicate role and posting assignments are not allowed. Check rows ' + rows + '.';
    }

    if (activeCount === 0) {
        return 'At least one active role and posting assignment is required.';
    }

    if (defaultRows.length === 0) {
        return 'Mark one active assignment as the default before saving.';
    }

    if (defaultRows.length > 1) {
        return 'Only one assignment can be marked as default. Check rows ' + defaultRows.join(', ') + '.';
    }

    if (inactiveDefaultRow) {
        return 'Default assignment row ' + inactiveDefaultRow + ' must be active.';
    }

    return null;
}

function buildSavePayload(isNewUser) {
    if ($('#passSaveField').val() !== $('#conPassSaveField').val()) {
        alert('Password and Confirm Password should be Same');
        return null;
    }

    var collectedAssignments = collectAssignmentsFromGrid();
    var validationMessage = validateAssignments(collectedAssignments);
    if (validationMessage) {
        alert(validationMessage);
        return null;
    }

    if (window.console && console.info) {
        console.info('Manage_user assignments payload', collectedAssignments.assignments);
    }

    return {
        USER_ID: isNewUser ? null : g_userId,
        PPNO: $.trim($('#ppnoSaveField').val()),
        PASSWORD: $('#passSaveField').val(),
        EMAIL_ADDRESS: $.trim($('#emailSaveField').val()),
        ISACTIVE: $('#isActiveSaveField').is(':checked') ? 'Y' : 'N',
        ASSIGNMENTS: collectedAssignments.assignments
    };
}

function persistUserContexts(payload, successMessage) {
    $.ajax({
        url: g_asiBaseURL + '/AdministrationPanel/save_user_contexts',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        cache: false,
        success: function (data) {
            showApiAlert(data, successMessage);
        },
        error: function (xhr) {
            showApiAlertFromXhr(xhr, xhr ? xhr.status : null, getErrorReferenceIdFromXhr(xhr), successMessage);
        },
        dataType: 'json'
    });
}

function saveButtonClickHandler() {
    var payload = buildSavePayload(false);
    if (!payload) {
        return;
    }

    persistUserContexts(payload, 'User Successfully Updated');
}

function addNewUserButtonClickHandler() {
    var payload = buildSavePayload(true);
    if (!payload) {
        return;
    }

    persistUserContexts(payload, 'User Successfully Created');
}

function checkBoxClicked() {
    if ($('#resetpassword').prop('checked')) {
        $('#passSaveField').val('Ztbl@1234');
        $('#conPassSaveField').val('Ztbl@1234');
    } else {
        $('#passSaveField').val('');
        $('#conPassSaveField').val('');
    }
}
