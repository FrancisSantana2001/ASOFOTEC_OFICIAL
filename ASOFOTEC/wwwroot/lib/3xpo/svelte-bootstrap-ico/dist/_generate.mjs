// must be executed with cwd as same dir as node modules
import fs from 'fs';

import { dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));

const icons = fs.readdirSync('node_modules/bootstrap-icons/icons/');
const snakeToCamel = str =>
  str
    .split('')
    .map((v, i, t) =>
      i === 0 && v.toLowerCase() === v.toUpperCase()
        ? `_${v}`
        : v === '_'
          ? ''
          : t[i - 1] === '_'
            ? v.toUpperCase()
            : v.toLowerCase(),
    )
    .join('');

const consts = [];

const typeFile = `// Bootstrap Icons Enum
export const enum Icon {
  ${[
    ...icons
      .map(v => v.replace('.svg', ''))
      .map(v => [v, snakeToCamel(v.replace(/[^a-zA-Z0-9_]/giu, '_'))])
      .map(v => `${v[1]} = ${JSON.stringify(v[0])},`),
    ...icons
      .map(v => v.replace('.svg', ''))
      .map(v => [v, v.replace(/[^a-zA-Z0-9_]/giu, '_')])
      .map(v => {
        consts.push(v[0]);
        return v;
      })
      .map(
        ([v0, v1]) =>
          `${
            v1.charAt(0).toLowerCase() === v1.charAt(0).toUpperCase() &&
            v1.charAt(0) !== '_'
              ? '_'
              : ''
          }${v1} = ${JSON.stringify(v0)},`,
      ),
  ]
    .filter((v, i, t) => t.indexOf(v) === i)
    .sort((a, b) => a.length - b.length)
    .join('\n  ')}
};
export type IconConsts = ${consts.map(v => JSON.stringify(v)).join('|')};
export type IconOrIconConsts = Icon | IconConsts;
export default Icon;
`;

fs.writeFileSync(__dirname + '/Icons.ts', typeFile);
