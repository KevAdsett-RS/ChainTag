"use strict";
// 1. Declare match handler functions separately
function matchInit(ctx, logger, nk, params) {
    return {
        state: {},
        tickRate: 1,
        label: params.label || '',
    };
}
function matchJoinAttempt(ctx, logger, nk, dispatcher, tick, state, presence, metadata) {
    return { state: state, accept: true };
}
function matchJoin(ctx, logger, nk, dispatcher, tick, state, presences) {
    return { state: state };
}
function matchLeave(ctx, logger, nk, dispatcher, tick, state, presences) {
    return { state: state };
}
function matchLoop(ctx, logger, nk, dispatcher, tick, state, messages) {
    messages.forEach(function (m) {
        dispatcher.broadcastMessage(m.opCode, m.data, [m.sender]);
    });
    return { state: state };
}
function matchTerminate(ctx, logger, nk, dispatcher, tick, state, graceSeconds) {
    return { state: state };
}
function matchSignal(ctx, logger, nk, dispatcher, tick, state, data) {
    return { state: state, data: data };
}
var matchHandler = {
    matchInit: matchInit,
    matchJoinAttempt: matchJoinAttempt,
    matchJoin: matchJoin,
    matchLeave: matchLeave,
    matchLoop: matchLoop,
    matchTerminate: matchTerminate,
    matchSignal: matchSignal,
};
function getOrCreateMatch(ctx, logger, nk, payload) {
    // 1. Search for any existing authoritative match of your type
    var limit = 1;
    var isAuthoritative = true;
    var labelFilter = '';
    var minSize = 0;
    var maxSize = 10;
    var query = '';
    var matches = nk.matchList(limit, isAuthoritative, labelFilter, minSize, maxSize, query);
    // 2. If a match already exists, return its ID immediately
    if (matches.length > 0) {
        logger.info('Found existing match: %s', matches[0].matchId);
        return JSON.stringify({ match_id: matches[0].matchId });
    }
    // 3. Otherwise, create a new one (acting as the host)
    var label = JSON.stringify({ host_ip: payload });
    var matchId = nk.matchCreate('chain_tag', { label: label });
    logger.info('No match found. Created new match: %s', matchId);
    return JSON.stringify({ match_id: matchId });
}
// Register the new function in InitModule
var InitModule = function (ctx, logger, nk, initializer) {
    initializer.registerMatch('chain_tag', matchHandler);
    initializer.registerRpc('get_or_create_match', getOrCreateMatch);
};
