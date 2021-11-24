import * as Actions from '../actions';

const initialGameState = {
    AIBoard: null,
    userBoard: null,
    AIprobabilities: null,
    username: '',
    userShips: [],
    AIShips: [],

    Errors: {
        wrongPlacedShip: null,
    }
};
const gameReducer = function (state = initialGameState, action) {
    switch (action.type) {
        case Actions.START_GAME: {
            var temp = new Array(6);
            for (var i = 0; i < temp.length; i++) {
                temp[i] = new Array(6);
            }
            for (var i = 0; i < 6; i++) {
                for (var j = 0; j < 6; j++) {
                    temp[i][j] = 0;
                }
            }
            return {
                AIBoard = temp,
                userBoard = temp,
            }
        }
        case Actions.GET_AI_SHIPS: {
            return {
                AIShips: { ...action.payload }
            }
        }
        case Actions.SET_SHIP: {
            return {
                userShips: userShips.push(action.payload),
            }
        }
        case Actions.WRONG_PLACED_SHIP: {
            return {
                Errors: {
                    wrongPlacedShip: true,
                }
            }
        }
        case Actions.GET_AI_PROBABILITIES: {
            return {
                AIprobabilities: {...action.payload},
            }
        }

    
        default:
            {
                return state;
            }
    }
};

export default gameReducer;