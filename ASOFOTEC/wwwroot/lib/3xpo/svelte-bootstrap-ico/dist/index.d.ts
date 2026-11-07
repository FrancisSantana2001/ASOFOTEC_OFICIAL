export { default, default as Bi, default as BI, default as Icon, } from './components/BI.svelte';
export { Icon as Icons, type IconOrIconConsts } from './Icons';
/** Component for use in <svelte:head> elements to import the font via jsdelivr - LoadFontNexus and/or manual loading is preferred over this */
export { default as LoadFontJsDelivr } from './components/LoadFontJsDelivr.svelte';
/** Component for use in <svelte:head> elements to import the font */
export { default as FontRemoteHeadImport } from './components/LoadFontNexus.svelte';
