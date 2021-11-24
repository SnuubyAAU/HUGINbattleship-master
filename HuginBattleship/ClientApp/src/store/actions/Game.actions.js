import axios from 'axios';
export const START_GAME = '[GAME] START GAME';
export const END_GAME = '[GAME] END GAME';
export const SET_SHIP = '[GAME] SET SHIP';
export const WRONG_PLACED_SHIP = '[GAME] WRONG PLACED SHIP';
export const GET_AI_SHIPS = '[GAME] GET AI PLACED SHIPS';

export const USER_SHOOT = '[GAME] USER SHOOT';
export const AI_SHOOT = '[GAME] AI SHOOT';
export const GET_AI_PROBABILITIES = '[GAME] GET AI PROBABILITIES';

export function StartGame(username) {
    axios.post('game/start', username);

    return (dispatch) => {
        dispatch({
            type: START_GAME,
            payload: username,
        })  
    }
}

export function GetAIShips() {
    const request = axios.get('game/get/AIships');

    return (dispatch) => {
        request.then((response) => {
            dispatch({
                type: GET_AI_SHIPS,
                payload: response.data,
            })
        });
    }
}
export function GetProbabilities() {
    const request = axios.get('game/get/probabilities');

    return (dispatch) => {
        request.then((response) => {
            dispatch({
                type: GET_AI_PROBABILITIES,
                payload: response.data,
            })
        });
    }
}
export function SetShip(ship) {
    const request = axios.post('game/setship', ship);
    return (dispatch) => {
        request.then((response) => {
            response.data = 'True' ? {
                dispatch({
                    type: SET_SHIP,
                    payload: ship,
                })
            } : {
                dispatch({
                    type: WRONG_PLACED_SHIP,
                })
            }
        });
    }
}
