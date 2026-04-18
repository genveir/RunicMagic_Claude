// ── Terminal ─────────────────────────────────────────────────────────────────

const term = new Terminal({
    cursorBlink: true,
    cursorStyle: 'block',
    theme: {
        background: '#111111',
        foreground: '#cccccc',
        cursor: '#cccccc',
    },
});

const fitAddon = new FitAddon.FitAddon();
term.loadAddon(fitAddon);
term.open(document.getElementById('terminal'));
term.onResize(() => term.scrollToBottom());

new ResizeObserver(() => fitAddon.fit()).observe(document.getElementById('terminal-container'));

function writePrompt(prompt) {
    term.write(prompt ?? '>');
}


// ── Input handling ────────────────────────────────────────────────────────────

let input        = '';
let cursorOffset = 0;
const history    = [];
let historyIndex = -1;

const ESC = '\x1b[';
const CURSOR_LEFT  = ESC + '1D';
const CURSOR_RIGHT = ESC + '1C';

const moveCursorLeft = n => n === 0 ? '' : `${ESC}${n}D`;

const clearInput = () =>
    moveCursorLeft(input.length - cursorOffset) +
    ' '.repeat(input.length) +
    moveCursorLeft(input.length);

term.onKey(({ key, domEvent }) => {
    switch (domEvent.key) {
        case 'Backspace': {
            const before = input.slice(0, input.length - cursorOffset);
            if (!before.length) return;
            const after = input.slice(input.length - cursorOffset);
            input = before.slice(0, -1) + after;
            term.write(
                CURSOR_LEFT +
                (after[0] ?? '') +
                after.slice(1) +
                ' ' +
                moveCursorLeft(cursorOffset + 1));
            break;
        }
        case 'Delete': {
            const after = input.slice(input.length - cursorOffset);
            if (!after.length) return;
            input = input.slice(0, input.length - cursorOffset) + after.slice(1);
            cursorOffset--;
            term.write(after.slice(1) + ' ' + moveCursorLeft(cursorOffset + 1));
            break;
        }
        case 'ArrowLeft':
            if (input.length - cursorOffset > 0) { cursorOffset++; term.write(CURSOR_LEFT); }
            break;
        case 'ArrowRight':
            if (cursorOffset > 0) { cursorOffset--; term.write(CURSOR_RIGHT); }
            break;
        case 'ArrowUp':
            if (historyIndex < history.length - 1) {
                historyIndex++;
                input = history[history.length - 1 - historyIndex];
                cursorOffset = 0;
                term.write(clearInput() + input);
            }
            break;
        case 'ArrowDown':
            if (historyIndex > 0) {
                historyIndex--;
                input = history[history.length - 1 - historyIndex];
                cursorOffset = 0;
                term.write(clearInput() + input);
            } else if (historyIndex === 0) {
                historyIndex = -1;
                input = '';
                cursorOffset = 0;
                term.write(clearInput());
            }
            break;
        case 'Enter':
            submitInput();
            break;
        default:
            if (key.charCodeAt(0) > 31 && key.charCodeAt(0) < 127) {
                const before = input.slice(0, input.length - cursorOffset);
                const after  = input.slice(input.length - cursorOffset);
                input = before + key + after;
                term.write(key + after + moveCursorLeft(cursorOffset));
            }
    }
});

async function submitInput() {
    const cmd = input;
    input        = '';
    cursorOffset = 0;
    historyIndex = -1;
    if (cmd.length) history.push(cmd);

    term.write('\r\n');

    const result = await sendCommand(cmd);

    for (const line of result.text) {
        if (line) term.writeln(line);
    }

    updateCanvas(result.entities);
    writePrompt(result.prompt);
    term.scrollToBottom();
}


// ── API ───────────────────────────────────────────────────────────────────────

async function sendCommand(cmd) {
    const response = await fetch('/command', {
        method:  'POST',
        headers: { 'Content-Type': 'application/json' },
        body:    JSON.stringify(cmd),
    });
    return response.json();
}


// ── Canvas ────────────────────────────────────────────────────────────────────

const SVG_NS = 'http://www.w3.org/2000/svg';
const svg    = document.getElementById('world-canvas');

const FLAGS_HAS_LIFE      = 1;
const FLAGS_HAS_AGENCY    = 2;
const FLAGS_IS_TRANSLUCENT = 4;

function entityClass(entity) {
    let cls;
    if ((entity.flags & FLAGS_HAS_LIFE) && (entity.flags & FLAGS_HAS_AGENCY)) cls = 'entity entity-creature';
    else if (entity.flags & FLAGS_HAS_LIFE)                                    cls = 'entity entity-life';
    else if (entity.flags & FLAGS_HAS_AGENCY)                                  cls = 'entity entity-agency';
    else                                                                        cls = 'entity entity-object';
    if (entity.isCaster)                        cls += ' entity-caster';
    if (entity.flags & FLAGS_IS_TRANSLUCENT)    cls += ' entity-translucent';
    if (entity.isIndicateTarget)                cls += ' entity-indicate-target';
    return cls;
}

function svgEl(tag, attrs) {
    const el = document.createElementNS(SVG_NS, tag);
    for (const [k, v] of Object.entries(attrs)) el.setAttribute(k, v);
    return el;
}

function updateCanvas(entities) {
    while (svg.firstChild) svg.removeChild(svg.firstChild);

    if (!entities.length) {
        svg.setAttribute('viewBox', '0 0 1000 1000');
        svg.appendChild(svgEl('text', {
            x: 500, y: 500,
            'dominant-baseline': 'middle',
            'text-anchor':       'middle',
            class:               'canvas-placeholder',
        })).textContent = 'no world loaded';
        return;
    }

    let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
    for (const e of entities) {
        minX = Math.min(minX,  e.x - e.width  / 2);
        minY = Math.min(minY, -e.y - e.height / 2);
        maxX = Math.max(maxX,  e.x + e.width  / 2);
        maxY = Math.max(maxY, -e.y + e.height / 2);
    }
    const pad = 100;
    svg.setAttribute('viewBox',
        `${minX - pad} ${minY - pad} ${maxX - minX + pad * 2} ${maxY - minY + pad * 2}`);

    for (const e of entities) {
        const g = svgEl('g', {});

        g.appendChild(svgEl('rect', {
            x: e.x - e.width / 2, y: -e.y - e.height / 2, width: e.width, height: e.height,
            class: entityClass(e),
        }));

        const label = svgEl('text', {
            x:                   e.x,
            y:                   -e.y,
            'dominant-baseline': 'middle',
            'text-anchor':       'middle',
            'font-size':         Math.max(e.height / 4, 12),
            class:               'entity-label',
        });
        label.textContent = e.label;
        g.appendChild(label);

        if (e.indicateEndX != null) {
            const sx = e.x,           sy = -e.y;
            const ex = e.indicateEndX, ey = -e.indicateEndY;

            g.appendChild(svgEl('line', {
                x1: sx, y1: sy, x2: ex, y2: ey,
                class: 'entity-indicate',
            }));

            const dx = ex - sx, dy = ey - sy;
            const len = Math.sqrt(dx * dx + dy * dy);
            if (len > 0) {
                const ux = dx / len, uy = dy / len;
                const px = -uy,      py = ux;
                const cLen = 150;
                g.appendChild(svgEl('path', {
                    d: `M ${ex - ux * cLen + px * cLen * 0.5},${ey - uy * cLen + py * cLen * 0.5}` +
                       ` L ${ex},${ey}` +
                       ` L ${ex - ux * cLen - px * cLen * 0.5},${ey - uy * cLen - py * cLen * 0.5}`,
                    class: 'entity-indicate',
                }));
            }
        }

        if (e.pointingEndX != null) {
            const sx = e.x,          sy = -e.y;
            const ex = e.pointingEndX, ey = -e.pointingEndY;

            g.appendChild(svgEl('line', {
                x1: sx, y1: sy, x2: ex, y2: ey,
                class: 'entity-direction',
            }));

            const dx = ex - sx, dy = ey - sy;
            const len = Math.sqrt(dx * dx + dy * dy);
            if (len > 0) {
                const ux = dx / len, uy = dy / len;
                const px = -uy,      py = ux;
                const cLen = 250;
                g.appendChild(svgEl('path', {
                    d: `M ${ex - ux * cLen + px * cLen * 0.5},${ey - uy * cLen + py * cLen * 0.5}` +
                       ` L ${ex},${ey}` +
                       ` L ${ex - ux * cLen - px * cLen * 0.5},${ey - uy * cLen - py * cLen * 0.5}`,
                    class: 'entity-direction',
                }));
            }
        }

        svg.appendChild(g);
    }
}


// ── Mode toggle ───────────────────────────────────────────────────────────────

let currentMode = null;

const modeButtons = {
    'pick-caster': document.getElementById('btn-pick-caster'),
    'move-caster': document.getElementById('btn-move-caster'),
    'point-at':    document.getElementById('btn-point-at'),
    'indicate':    document.getElementById('btn-indicate'),
};

function setMode(mode) {
    currentMode = mode;
    for (const [key, btn] of Object.entries(modeButtons)) {
        btn.classList.toggle('active', key === mode);
    }
}

for (const [mode, btn] of Object.entries(modeButtons)) {
    btn.addEventListener('click', () => {
        setMode(currentMode === mode ? null : mode);
    });
}

document.addEventListener('contextmenu', e => {
    if (!currentMode) return;
    e.preventDefault();
    setMode(null);
});

svg.addEventListener('click', async e => {
    if (!currentMode) return;

    const pt = svg.createSVGPoint();
    pt.x = e.clientX;
    pt.y = e.clientY;
    const svgPt = pt.matrixTransform(svg.getScreenCTM().inverse());

    const mode = currentMode;
    setMode(null);

    const result = await sendModeClick(mode, svgPt.x, -svgPt.y);

    term.write('\r\n');
    for (const line of result.text) {
        if (line) term.writeln(line);
    }
    updateCanvas(result.entities);
    writePrompt(result.prompt);
    term.scrollToBottom();
});

async function sendModeClick(mode, x, y) {
    const response = await fetch(`/${mode}`, {
        method:  'POST',
        headers: { 'Content-Type': 'application/json' },
        body:    JSON.stringify({ x, y }),
    });
    return response.json();
}


// ── Divider drag ──────────────────────────────────────────────────────────────

const divider           = document.getElementById('divider');
const terminalContainer = document.getElementById('terminal-container');

let dragging           = false;
let dragStartY         = 0;
let dragStartHeight    = 0;

divider.addEventListener('mousedown', e => {
    dragging        = true;
    dragStartY      = e.clientY;
    dragStartHeight = terminalContainer.offsetHeight;
    document.body.style.cursor    = 'ns-resize';
    document.body.style.userSelect = 'none';
    e.preventDefault();
});

document.addEventListener('mousemove', e => {
    if (!dragging) return;
    const delta     = dragStartY - e.clientY;          // drag up → terminal grows
    const newHeight = Math.max(50, Math.min(window.innerHeight - 100, dragStartHeight + delta));
    terminalContainer.style.height = newHeight + 'px';
});

document.addEventListener('mouseup', () => {
    if (!dragging) return;
    dragging                       = false;
    document.body.style.cursor     = '';
    document.body.style.userSelect = '';
});


// ── Init ──────────────────────────────────────────────────────────────────────

async function init() {
    const entities = await fetch('/world').then(r => r.json());
    updateCanvas(entities);
    writePrompt('[no caster] >');
}

init();
