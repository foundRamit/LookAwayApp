import cv2
import numpy as np
import time
import threading
import queue
from tkinter import Tk, Label, Button, StringVar, Frame, Scale, HORIZONTAL
from PIL import Image, ImageTk

class LookawayApp:
    def __init__(self):
        # Initialize UI
        self.root = Tk()
        self.root.title("Lookaway - Proof of Concept")
        self.root.geometry("500x400")
        self.root.resizable(False, False)
        
        # App state
        self.running = False
        self.status_text = StringVar()
        self.status_text.set("Click Start to begin monitoring")
        self.look_away_threshold = 1.0  # seconds
        self.last_seen_time = time.time()
        self.is_looking = True
        self.blur_active = False
        
        # Create a queue for thread-safe UI updates
        self.command_queue = queue.Queue()
        
        # Create UI elements
        self.create_ui()
        
        # Face detection setup
        self.face_cascade = cv2.CascadeClassifier(cv2.data.haarcascades + 'haarcascade_frontalface_default.xml')
        self.eye_cascade = cv2.CascadeClassifier(cv2.data.haarcascades + 'haarcascade_eye.xml')
        
        # Camera setup
        self.cap = None
        self.camera_thread = None
        
        # Create blur overlay window
        self.create_blur_overlay()
        
        # Set up periodic check for command queue
        self.root.after(100, self.check_command_queue)
        
    def create_ui(self):
        # Status frame
        status_frame = Frame(self.root, padx=10, pady=10)
        status_frame.pack(fill='x')
        
        Label(status_frame, text="Status:").pack(side='left')
        Label(status_frame, textvariable=self.status_text).pack(side='left', padx=5)
        
        # Preview frame
        self.preview_frame = Frame(self.root, padx=10, pady=10)
        self.preview_frame.pack(fill='both', expand=True)
        
        self.preview_label = Label(self.preview_frame)
        self.preview_label.pack(fill='both', expand=True)
        
        # Settings frame
        settings_frame = Frame(self.root, padx=10, pady=5)
        settings_frame.pack(fill='x')
        
        Label(settings_frame, text="Look Away Threshold (seconds):").pack(side='left')
        self.threshold_scale = Scale(settings_frame, from_=0.5, to=5.0, 
                                    resolution=0.5, orient=HORIZONTAL,
                                    command=self.update_threshold)
        self.threshold_scale.set(1.0)
        self.threshold_scale.pack(side='left', padx=5, fill='x', expand=True)
        
        # Control frame
        control_frame = Frame(self.root, padx=10, pady=10)
        control_frame.pack(fill='x')
        
        self.start_button = Button(control_frame, text="Start", width=10, command=self.start_monitoring)
        self.start_button.pack(side='left', padx=5)
        
        self.stop_button = Button(control_frame, text="Stop", width=10, command=self.stop_monitoring, state='disabled')
        self.stop_button.pack(side='left', padx=5)
        
        self.exit_button = Button(control_frame, text="Exit", width=10, command=self.exit_app)
        self.exit_button.pack(side='right', padx=5)
        
    def create_blur_overlay(self):
        # Create a transparent window to overlay
        self.overlay_window = Tk()
        self.overlay_window.title("Blur Overlay")
        self.overlay_window.attributes('-alpha', 0.0)  # Start transparent
        self.overlay_window.attributes('-topmost', True)  # Keep on top
        self.overlay_window.attributes('-fullscreen', True)  # Fullscreen
        self.overlay_window.withdraw()  # Hide initially
        
        # Create a semi-transparent blur label
        self.blur_label = Label(self.overlay_window, bg='black')
        self.blur_label.pack(fill='both', expand=True)
    
    def check_command_queue(self):
        # Process all pending commands in the queue
        try:
            while True:
                cmd, args = self.command_queue.get_nowait()
                if cmd == 'update_status':
                    self.status_text.set(args)
                elif cmd == 'activate_blur':
                    self._activate_blur()
                elif cmd == 'deactivate_blur':
                    self._deactivate_blur()
                elif cmd == 'update_preview':
                    self._update_preview(args)
                self.command_queue.task_done()
        except queue.Empty:
            pass
        
        # Schedule the next check
        if self.running:
            self.root.after(100, self.check_command_queue)
    
    def _activate_blur(self):
        if not self.blur_active:
            self.blur_active = True
            self.overlay_window.deiconify()  # Show overlay
            self.overlay_window.attributes('-alpha', 0.8)  # Set semi-transparent
            self.blur_label.config(text="Screen blurred - Look at screen to continue")
            self.overlay_window.update()
    
    def _deactivate_blur(self):
        if self.blur_active:
            self.blur_active = False
            self.overlay_window.attributes('-alpha', 0.0)  # Make transparent
            self.overlay_window.withdraw()  # Hide overlay
            self.overlay_window.update()
    
    def _update_preview(self, frame):
        # Convert frame to format for tkinter
        cv2image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGBA)
        img = Image.fromarray(cv2image)
        
        # Resize for preview
        preview_width = 320
        preview_height = 240
        img = img.resize((preview_width, preview_height), Image.LANCZOS)
        
        # Convert to PhotoImage
        imgtk = ImageTk.PhotoImage(image=img)
        
        # Update preview label
        self.preview_label.imgtk = imgtk
        self.preview_label.config(image=imgtk)
        
    def update_threshold(self, value):
        self.look_away_threshold = float(value)
        
    def start_monitoring(self):
        if not self.running:
            self.running = True
            self.start_button.config(state='disabled')
            self.stop_button.config(state='normal')
            self.status_text.set("Starting camera...")
            
            # Start camera in a separate thread
            self.cap = cv2.VideoCapture(0)
            self.camera_thread = threading.Thread(target=self.process_camera)
            self.camera_thread.daemon = True
            self.camera_thread.start()
            
            # Re-start the command queue checker
            self.root.after(100, self.check_command_queue)
            
    def stop_monitoring(self):
        if self.running:
            self.running = False
            self.start_button.config(state='normal')
            self.stop_button.config(state='disabled')
            self.status_text.set("Monitoring stopped")
            
            # Release camera
            if self.cap:
                self.cap.release()
                
            # Hide blur overlay if active
            self._deactivate_blur()
            
    def exit_app(self):
        self.stop_monitoring()
        self.root.quit()
        self.overlay_window.destroy()
        
    def process_camera(self):
        if not self.cap.isOpened():
            self.command_queue.put(('update_status', "Error: Could not open camera"))
            self.stop_monitoring()
            return
        
        while self.running:
            ret, frame = self.cap.read()
            if not ret:
                self.command_queue.put(('update_status', "Error: Could not read frame"))
                self.stop_monitoring()
                break
                
            # Convert to grayscale for face detection
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            
            # Detect faces
            faces = self.face_cascade.detectMultiScale(gray, 1.3, 5)
            
            # Check if user is looking at screen
            if len(faces) > 0:
                # Found a face, now check for eyes
                for (x, y, w, h) in faces:
                    # Draw rectangle around face in preview
                    cv2.rectangle(frame, (x, y), (x+w, y+h), (255, 0, 0), 2)
                    
                    # Region of interest for eye detection
                    roi_gray = gray[y:y+h, x:x+w]
                    roi_color = frame[y:y+h, x:x+w]
                    
                    # Detect eyes
                    eyes = self.eye_cascade.detectMultiScale(roi_gray)
                    
                    # If eyes detected, consider user is looking
                    if len(eyes) > 0:
                        # Draw circles around eyes in preview
                        for (ex, ey, ew, eh) in eyes:
                            cv2.circle(roi_color, (ex + ew//2, ey + eh//2), 5, (0, 255, 0), -1)
                        
                        # User is looking at screen
                        if not self.is_looking:
                            self.is_looking = True
                            self.command_queue.put(('deactivate_blur', None))
                            self.command_queue.put(('update_status', "User is looking at screen"))
                        
                        # Update last seen time
                        self.last_seen_time = time.time()
                    else:
                        # No eyes detected, check if threshold exceeded
                        self.check_look_away_threshold()
                        
            else:
                # No face detected, check if threshold exceeded
                self.check_look_away_threshold()
            
            # Update preview through the command queue
            self.command_queue.put(('update_preview', frame.copy()))
            
            # Short delay to reduce CPU usage
            time.sleep(0.05)
            
    def check_look_away_threshold(self):
        # Check if enough time has passed since last seeing user
        elapsed_time = time.time() - self.last_seen_time
        if elapsed_time >= self.look_away_threshold and self.is_looking:
            self.is_looking = False
            self.command_queue.put(('activate_blur', None))
            self.command_queue.put(('update_status', f"User looked away for {elapsed_time:.1f} seconds"))
            
    def activate_blur(self):
        # Thread-safe version that uses the command queue
        self.command_queue.put(('activate_blur', None))
            
    def deactivate_blur(self):
        # Thread-safe version that uses the command queue
        self.command_queue.put(('deactivate_blur', None))
            
    def update_preview(self, frame):
        # Thread-safe version that uses the command queue
        self.command_queue.put(('update_preview', frame))
        
    def run(self):
        self.root.mainloop()
        
if __name__ == "__main__":
    app = LookawayApp()
    app.run()