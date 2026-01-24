<template>
  <div class="space-y-4">
    <!-- Table skeleton -->
    <div v-if="type === 'table'" class="card">
      <div class="overflow-x-auto">
        <table class="min-w-full">
          <thead>
            <tr class="bg-gray-50 border-b">
              <th v-for="i in columns" :key="i" class="px-6 py-3">
                <div class="skeleton h-4 w-20"></div>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row" class="border-b">
              <td v-for="col in columns" :key="col" class="px-6 py-4">
                <div class="skeleton h-4" :class="getRandomWidth()"></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    
    <!-- Card skeleton -->
    <div v-else-if="type === 'card'" class="grid gap-4" :class="gridClass">
      <div v-for="i in count" :key="i" class="card p-6">
        <div class="skeleton h-6 w-3/4 mb-4"></div>
        <div class="skeleton h-4 w-full mb-2"></div>
        <div class="skeleton h-4 w-5/6 mb-4"></div>
        <div class="flex gap-2">
          <div class="skeleton h-6 w-16 rounded-full"></div>
          <div class="skeleton h-6 w-20 rounded-full"></div>
        </div>
      </div>
    </div>
    
    <!-- List skeleton -->
    <div v-else-if="type === 'list'" class="space-y-3">
      <div v-for="i in count" :key="i" class="card p-4 flex items-center gap-4">
        <div class="skeleton h-12 w-12 rounded-full"></div>
        <div class="flex-1">
          <div class="skeleton h-4 w-1/3 mb-2"></div>
          <div class="skeleton h-3 w-1/2"></div>
        </div>
      </div>
    </div>
    
    <!-- Stats skeleton -->
    <div v-else-if="type === 'stats'" class="grid gap-4" :class="gridClass">
      <div v-for="i in count" :key="i" class="card p-6">
        <div class="flex items-center justify-between">
          <div>
            <div class="skeleton h-4 w-24 mb-2"></div>
            <div class="skeleton h-8 w-32"></div>
          </div>
          <div class="skeleton h-12 w-12 rounded-lg"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  type: {
    type: String,
    default: 'table',
    validator: (value) => ['table', 'card', 'list', 'stats'].includes(value)
  },
  rows: {
    type: Number,
    default: 5
  },
  columns: {
    type: Number,
    default: 5
  },
  count: {
    type: Number,
    default: 4
  },
  gridCols: {
    type: Number,
    default: 2
  }
})

const gridClass = computed(() => {
  const cols = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 md:grid-cols-2',
    3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
    4: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-4'
  }
  return cols[props.gridCols] || 'grid-cols-2'
})

const getRandomWidth = () => {
  const widths = ['w-16', 'w-20', 'w-24', 'w-28', 'w-32']
  return widths[Math.floor(Math.random() * widths.length)]
}
</script>
