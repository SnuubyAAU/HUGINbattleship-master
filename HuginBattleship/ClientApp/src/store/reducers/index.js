import { combineReducers } from 'redux';
import { game } from '../reducers/game.reducers'
const reducer = combineReducers({
    game
});
export default reducer;