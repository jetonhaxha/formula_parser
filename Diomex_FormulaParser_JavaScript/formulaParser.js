/*                   Formula Parser                          */
// Used Shunting Yard Algorithm (RPN - Reverse Polish Notation)
// Prerequisities: node.js needs to be installed
// How to run: in the root directory type 'node formulaParser'

class FormulaParser {
    constructor() {
        this.operators = {
            '+': { precedence: 1, operation: (a, b) => a + b },
            '-': { precedence: 1, operation: (a, b) => a - b },
            '*': { precedence: 2, operation: (a, b) => a * b },
            '/': { precedence: 2, operation: (a, b) => a / b }
        };
        this.functions = {
            'Str': (value) => String(value),
            'Val': (value) => parseFloat(value),
        };
        this.variables = {};
    }

    setVariable(name, value) {
        this.variables[name] = value;
    }

    tokenize(formula) {
        const regex = /\s*([a-zA-Z]+|[0-9]+\.[0-9]+|[0-9]+|\S)\s*/g;
        const tokens = [];
        let match;

        while ((match = regex.exec(formula)) !== null) {
            tokens.push(match[1]);
        }
        return tokens;
    }

    parseFormula(formula) {
        const outputQueue = [];
        const operatorStack = [];

        const tokens = this.tokenize(formula);

        // Shunting Yard Algorithm
        tokens.forEach((token) => {
            if (!isNaN(token)) {
                outputQueue.push(parseFloat(token));
            } else if (token in this.variables) {
                outputQueue.push(this.variables[token]);
            } else if (token in this.functions) {
                operatorStack.push(token);
            } else if (token === '(') {
                operatorStack.push(token);
            } else if (token === ')') {
                while (operatorStack.length > 0 && operatorStack[operatorStack.length - 1] !== '(') {
                    outputQueue.push(operatorStack.pop());
                }
                operatorStack.pop(); // Remove the '('
                // If the token is a function, then push into queue
                while (operatorStack.length > 0 && operatorStack[operatorStack.length - 1] in this.functions) {
                    outputQueue.push(operatorStack.pop());
                }
            } else if (token in this.operators) {
                while (
                    operatorStack.length > 0 &&
                    this.operators[token].precedence <= this.operators[operatorStack[operatorStack.length - 1]]?.precedence
                ) {
                    outputQueue.push(operatorStack.pop());
                }
                operatorStack.push(token);
            } else if (token !== '"') {
                outputQueue.push(token);
            }
        });
        while (operatorStack.length > 0) {
            outputQueue.push(operatorStack.pop());
        }
        return outputQueue;
    }

    evaluateRPN(rpn) {
        const stack = [];
        rpn.forEach((token) => {
            if (token in this.operators) {
                const b = stack.pop();
                const a = stack.pop();
                stack.push(this.operators[token].operation(a, b));
            } else if (token in this.functions) {
                const arg = stack.pop();
                stack.push(this.functions[token](arg));
            } else {
                stack.push(token);
            }
        });

        return stack[0];
    }

    calculate(formula) {
        const rpn = this.parseFormula(formula);
        return this.evaluateRPN(rpn);
    }
}

// New instance of the class
const parser = new FormulaParser();

// Create variables
parser.setVariable('sum', 10);
parser.setVariable('name', 'John');

// Examples
const example1 = parser.calculate('1 + 2.5 * 3');
const example2 = parser.calculate('((1+2) * (3 + 4)) + (5.2 * sum)');
const example3 = parser.calculate('"Hello " + name');
const example4 = parser.calculate('Str((1 + 2) * Val("55")) + sum');

console.log('Result: ' + example1);
console.log('Result: ' + example2);
console.log('Result: ' + example3);
console.log('Result: ' + example4);
