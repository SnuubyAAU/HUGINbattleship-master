import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import { ApplicationState } from '../store';
import * as GameStore from '../store/Game';
import './Game.css'
type GameProps =
    GameStore.GameState &
    typeof GameStore.actionCreators &
    RouteComponentProps<{}>;

class Game extends React.PureComponent<GameProps> {
    constructor(props: GameProps) {
        super(props);

        this.CreateTiles = this.CreateTiles.bind(this);
    }
    public CreateTiles = () => {
        var AIboard = this.props.AIBoard;
        AIboard = [];
        for (var i = 0; i < 6; i++) {
            AIboard[i] = [];
            for (var j = 0; j < 6; j++) {
                AIboard[i][j] = { hit: false, isShot: false, probability: 0 }; // Creates all the tiles as interfaces
            }
        }

        var userBoard = this.props.userBoard;
        userBoard = [];
        for (var i = 0; i < 6; i++) {
            userBoard[i] = [];
            for (var j = 0; j < 6; j++) {
                userBoard[i][j] = { hit: false, isShot: false, probability: 0 }; // Creates all the tiles as interfaces
            }
        }


        this.props.makeBoards(userBoard, AIboard);

    }
    renderUserboard() {
        // Build the rows in an array
        let rows = [];
        let userBoard = this.props.AIBoard;
        for (let y = 0; y < 6; y++) {
            // Build the cells in an array
            const cells = [];
            for (let x = 0; x < 6; x++) {
                cells.push(<Cell tile={userBoard![y][x]} />);
            }
            // Put them in the row
            rows.push(<tr>{cells}</tr>);
        }
        // Return the table
        return <table><tbody>{rows}</tbody></table>;
    }
    renderAIboard() {
        // Build the rows in an array
        let rows = [];
        let AIboard = this.props.AIBoard;
        for (let y = 0; y < 6; y++) {
            // Build the cells in an array
            const cells = [];
            for (let x = 0; x < 6; x++) {
                cells.push(<Cell tile={AIboard![y][x]} />);
            }
            // Put them in the row
            rows.push(<tr>{cells}</tr>);
        }
        // Return the table
        return <table><tbody>{rows}</tbody></table>;
    }

    render() {
        return (
            <React.Fragment>
                {this.CreateTiles()}
                {this.renderAIboard()}
                {this.renderUserboard()}
            </React.Fragment>
        )
    }
};




interface CellState {
    hit: boolean,
    shot: boolean,
}
class Cell extends React.Component<any, CellState> {
    constructor(props: any) {
        super(props);

        this.state = {
            hit: props.hit!,
            shot: props.isShot!,
        }

    }

    render() {
        return (
            <td className="cell" color={this.props.hit ? "white" : "black"}></td>
        )
    }
    
}


export default connect()(Game);
