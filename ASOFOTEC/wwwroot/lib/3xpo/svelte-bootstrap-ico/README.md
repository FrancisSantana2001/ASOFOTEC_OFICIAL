<div align="center">

# ![Svelte Bootstrap Ico](https://codeberg.org/Expo/Svelte-BI/raw/branch/senpai/static/svelte-bi.png)

[![📦 NPM](https://img.shields.io/npm/v/%403xpo/svelte-bootstrap-ico?label=📦%20NPM)](https://npmjs.com/package/@3xpo/svelte-bootstrap-ico)
<br/>
[![📦 Bundle Size](https://img.shields.io/bundlephobia/min/%403xpo/svelte-bootstrap-ico?label=📦%20Bundle%20Size)](https://bundlephobia.com/package/@3xpo/svelte-bootstrap-ico)
[![📁 Source Size](https://img.shields.io/maintenance/yes/2024?label=📁%20Maintained)](https://codeberg.org/Expo/svelte-bi/)<br/>
A utility for using [Svelte](https://svelte.dev) with Bootstrap Icons' Font Loading Mechanism.

</div>

## Recommended

I recommend using this with [Purgecss](https://www.skeleton.dev/docs/purgecss) to eliminate all of the unneeded classes. I also recommend using it with NexusFonts, as described below.

## Enums

### Icons

This is the only enum. It exports both camelCase and snake_case'd versions of all of the icons.

## Components

### BI

The `BI`/`Bootstrap Icon` component is the main component, and it creates the `i` element with your icon.

It takes an `icon` property, which is either one of the icons' `dash-separated-names`, or an item in the `Icons` enum.

### LoadFontNexus

Loads the font from [NexusFonts](https://fonts.arson.wtf/docs/) - via the [`Bootstrap Icons`](https://fonts.arson.wtf/css?family=Bootstrap+Icons) Font Family.

> **Note**
> If you want the performance of NexusFonts, yet want to still have Purgecss be able to trim the stylesheet, you can load the `.scss` file manually & specify `$bootstrap-icons-font-src` as `https://fonts.arson.wtf/api/v1/download/Bootstrap%20Icons.woff2`. More information on specifying css import variables can be found [here](https://sass-lang.com/documentation/at-rules/use/#configuration).

> **Note**
> If you're importing other NexusFonts Fonts, you can just add the `Bootstrap Icons` family to your query directly - e.g. by appending `&family=Bootstrap+Icons` to the end - If you're not, you should [check NexusFonts out](https://fonts.arson.wtf/docs/) :D

### Loading manually via CSS/SCSS

If you have the `bootstrap-icons` package installed, you can use one of the following:

```html
<script lang="ts">
  import 'bootstrap-icons/font/bootstrap-icons.scss';
  // or
  import 'bootstrap-icons/font/bootstrap-icons.min.css';
</script>
```

### LoadFontJsDelivr

The `LoadFontJsDelivr` component imports Bootstrap Icons from jsdelivr. You can place it in your head if you prefer to avoid the above 2 options.
