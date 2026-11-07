import { SvelteComponent } from "svelte";
import type { IconOrIconConsts } from '../Icons.js';
declare const __propDef: {
    props: {
        [x: string]: any;
        icon: IconOrIconConsts;
        style?: string | undefined;
        class?: string | undefined;
    };
    events: {
        [evt: string]: CustomEvent<any>;
    };
    slots: {
        default: {};
    };
};
export type BiProps = typeof __propDef.props;
export type BiEvents = typeof __propDef.events;
export type BiSlots = typeof __propDef.slots;
export default class Bi extends SvelteComponent<BiProps, BiEvents, BiSlots> {
}
export {};
