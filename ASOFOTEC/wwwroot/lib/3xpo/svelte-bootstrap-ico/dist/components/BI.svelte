<!-- Bootstrap Icons Component -->
<script>export let icon;
export let style = "";
let className = "";
export { className as class };
</script>

<i
  class="bi bi-{icon}{className ? ` ${className}` : ''}"
  {style}
  {...$$restProps}
>
  <slot />
</i>
