import { Action, Reducer } from 'redux';
import axios from 'axios';
import { AppThunkAction } from '.';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface GameState {
    AIBoard?: Tile[][],
    userBoard?: Tile[][],
    AIprobabilities?: Tile[][],
    username?: string,
    userShips?: Ship[],
    AIShips?: Ship[],
    AIshots?: Coord[],
    userShot?: Coord[],
    gameState?: {
        begun?: boolean,
        ended?: boolean,
        wasHit?: boolean,
    }
    errors?: {
        wrongPlacedShip: boolean,
    }

}
export interface Ship {
    length: number,
    hits?: number,
    name: string,
    orientation: string,
    x: number,
    y: number,
}
export interface Coord {
    x: number,
    y: number,
}
export interface ShotInfo {
    hit: boolean, 
    gameOver: boolean,
    coord: Coord,
}
export interface Tile {
    isShot?: boolean,
    hit?: boolean,
   probability?: number,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export interface StartGameAction { type: '[GAME] START GAME', username: string}
export interface MakeBoardsAction { type: '[GAME] MAKE BOARDS', userBoard: Tile[][], AIboard: Tile[][] }

export interface EndGameAction { type: '[GAME] END GAME' }
export interface SetShipAction { type: '[GAME] SET SHIP', ship: Ship }
export interface WrongPlacedShipAction { type: '[GAME] WRONG PLACED SHIP' }
export interface GetAIShipsAction { type: '[GAME] GET AI SHIPS', AIships: Ship[] }
export interface UserShootAction { type: '[GAME] USER SHOOT', coord: Coord, info: ShotInfo}
export interface AIShootAction { type: '[GAME] AI SHOOT', info: ShotInfo }
export interface GetAIProbabilities { type: '[GAME] GET AI PROBABILITIES', tiles: Tile[][] }

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
export type KnownAction = StartGameAction | MakeBoardsAction |EndGameAction | SetShipAction | WrongPlacedShipAction |
    GetAIShipsAction | UserShootAction | AIShootAction | GetAIProbabilities;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

    //Starting game action
    startGame: (username: string): AppThunkAction<KnownAction> => (dispatch) => {
        axios.post(`game/start`, username);
        dispatch({ type: '[GAME] START GAME', username: username});
    },

    makeBoards: (userBoard: Tile[][], AIboard: Tile[][]): AppThunkAction<KnownAction> => (dispatch) => {
        dispatch({ type: '[GAME] MAKE BOARDS', userBoard: userBoard, AIboard: AIboard });
    },

    endGame: (): AppThunkAction<KnownAction> => (dispatch) => {
        axios.post(`game/end`);
        dispatch({ type: '[GAME] END GAME'});
    },
    // Set ship action
    setShip: (ship: Ship): AppThunkAction<KnownAction> => (dispatch) => {
        const request = axios.post(`game/setship`, ship);
        request.then((response: { data: boolean; }) => { 
            if (response.data == true) {
                dispatch({ type: '[GAME] SET SHIP', ship: ship });
            }
            else {
                dispatch({ type: '[GAME] WRONG PLACED SHIP' });
            }
        });
    },
    // Get AI placed ships (Ships on user's board) action
    getAIShips: (): AppThunkAction<KnownAction> => (dispatch) => {
        const request = axios.get(`game/get/AIships`);
        request.then((response: { data: Ship[]; }) => {
            dispatch({ type: '[GAME] GET AI SHIPS', AIships: response.data });
        });
    },
    // User shoot action
    userShoot: (coord: Coord): AppThunkAction<KnownAction> => (dispatch) => {
        const request = axios.post(`game/human/shoot`, coord);
        request.then((response: { data: ShotInfo; }) => {
            dispatch({ type: '[GAME] USER SHOOT', coord: coord, info: response.data});
        });
    },
    // AI shoot action
    AIShoot: (): AppThunkAction<KnownAction> => (dispatch) => {
        const request = axios.post(`game/ai/shoot`);
        request.then((response: { data: ShotInfo; }) => {
            dispatch({ type: '[GAME] AI SHOOT', info: response.data });
        });
    },
    // Get tiles info action
    getAIProbabilities: (): AppThunkAction<KnownAction> => (dispatch) => {
        const request = axios.get('game/get/probabilities');
        request.then((response: { data: Tile[][]; }) => {
            dispatch({ type: '[GAME] GET AI PROBABILITIES', tiles: response.data });
        });
    },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const defaultState: GameState = {
    username: '',
    AIShips: [],
    gameState: {
        begun: false,
        ended: false
    },
    errors: {
        wrongPlacedShip: false,
    }
};

export const reducer: Reducer<GameState> = (state: GameState | undefined , incomingAction: Action): GameState => {
    if (state === undefined) {
        return defaultState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case '[GAME] START GAME':
            return {
                gameState: {
                    begun: true,
                },
                username: action.username,
            };
        case '[GAME] MAKE BOARDS':
            return Object.assign({}, state, {
                userBoard: action.userBoard,
                AIboard: action.AIboard,
            });
        case '[GAME] END GAME':
            return {
                gameState: {
                    ended: true
                },
            };
        case '[GAME] SET SHIP':
            const ship = action.ship;
            return Object.assign({}, state, {
                userShips: [...state.userShips!, ship]
            });

        case '[GAME] WRONG PLACED SHIP':
            return {
                ...state,
                errors: {
                    wrongPlacedShip: true
                },
            };
        case '[GAME] USER SHOOT':
            return Object.assign({}, state, {
                userShot: [...state.userShot!, action.coord],
                gameState: {
                    ended: action.info.gameOver,
                    wasHit: action.info.hit
                }
            });
        case '[GAME] AI SHOOT':

            return Object.assign({}, state, {
                AIshoot: [...state.userShot!, action.info.coord],
                gameState: {
                    ended: action.info.gameOver,
                    wasHit: action.info.hit,
                }
            });
        case '[GAME] GET AI PROBABILITIES':

            return Object.assign({}, state, {
                AIprobabilitites: action.tiles
            });
        default:
            return state;
    }
};
