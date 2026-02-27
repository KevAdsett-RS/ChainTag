// 1. Declare match handler functions separately
function matchInit(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  params: { [key: string]: any },
): { state: nkruntime.MatchState; tickRate: number; label: string } {
  return {
    state: {},
    tickRate: 1,
    label: params.label || '',
  }
}

function matchJoinAttempt(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  dispatcher: nkruntime.MatchDispatcher,
  tick: number,
  state: nkruntime.MatchState,
  presence: nkruntime.Presence,
  metadata: { [key: string]: any },
): { state: nkruntime.MatchState; accept: boolean; rejectMessage?: string } {
  return { state, accept: true }
}

function matchJoin(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  dispatcher: nkruntime.MatchDispatcher,
  tick: number,
  state: nkruntime.MatchState,
  presences: nkruntime.Presence[],
): { state: nkruntime.MatchState } {
  return { state }
}

function matchLeave(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  dispatcher: nkruntime.MatchDispatcher,
  tick: number,
  state: nkruntime.MatchState,
  presences: nkruntime.Presence[],
): { state: nkruntime.MatchState } {
  return { state }
}

function matchLoop(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  dispatcher: nkruntime.MatchDispatcher,
  tick: number,
  state: nkruntime.MatchState,
  messages: nkruntime.MatchMessage[],
): { state: nkruntime.MatchState } {
  messages.forEach((m) => {
    dispatcher.broadcastMessage(m.opCode, m.data, [m.sender])
  })
  return { state }
}

function matchTerminate(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  dispatcher: nkruntime.MatchDispatcher,
  tick: number,
  state: nkruntime.MatchState,
  graceSeconds: number,
): { state: nkruntime.MatchState } {
  return { state }
}

function matchSignal(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  dispatcher: nkruntime.MatchDispatcher,
  tick: number,
  state: nkruntime.MatchState,
  data: string,
): { state: nkruntime.MatchState; data?: string } {
  return { state, data }
}

const matchHandler: nkruntime.MatchHandler = {
  matchInit,
  matchJoinAttempt,
  matchJoin,
  matchLeave,
  matchLoop,
  matchTerminate,
  matchSignal,
}

function getOrCreateMatch(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string,
): string {
  // 1. Search for any existing authoritative match of your type
  const limit = 1
  const isAuthoritative = true
  const labelFilter = ''
  const minSize = 0
  const maxSize = 10
  const query = ''

  const matches = nk.matchList(
    limit,
    isAuthoritative,
    labelFilter,
    minSize,
    maxSize,
    query,
  )

  // 2. If a match already exists, return its ID immediately
  if (matches.length > 0) {
    logger.info('Found existing match: %s', matches[0].matchId)
    return JSON.stringify({ match_id: matches[0].matchId })
  }

  // 3. Otherwise, create a new one (acting as the host)
  const label = JSON.stringify({ host_ip: payload })
  logger.info('Label for new match created: %s', label)
  const matchId = nk.matchCreate('chain_tag', { label: label })

  logger.info('No match found. Created new match: %s', matchId)
  return JSON.stringify({ match_id: matchId })
}

// Register the new function in InitModule
let InitModule: nkruntime.InitModule = function (ctx, logger, nk, initializer) {
  initializer.registerMatch('chain_tag', matchHandler)
  initializer.registerRpc('get_or_create_match', getOrCreateMatch)
}
