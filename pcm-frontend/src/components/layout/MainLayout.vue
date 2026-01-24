<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Mobile sidebar overlay -->
    <Transition name="fade">
      <div 
        v-if="sidebarOpen" 
        class="fixed inset-0 bg-black bg-opacity-50 z-40 lg:hidden"
        @click="sidebarOpen = false"
      ></div>
    </Transition>
    
    <!-- Sidebar -->
    <Sidebar 
      :open="sidebarOpen" 
      @close="sidebarOpen = false" 
    />
    
    <!-- Main content -->
    <div class="lg:pl-64">
      <!-- Navbar -->
      <Navbar @toggle-sidebar="sidebarOpen = !sidebarOpen" />
      
      <!-- Page content -->
      <main class="p-4 lg:p-6">
        <slot></slot>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import Navbar from './Navbar.vue';
import Sidebar from './Sidebar.vue';

const sidebarOpen = ref(false)
</script>
